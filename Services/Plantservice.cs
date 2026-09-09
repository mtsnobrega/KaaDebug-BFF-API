/*
 * Responsabilidade:
 * Serviço central de gerenciamento de Plantas e coordenação de hardwares IoT (Devices).
 *
 * Regras de Negócio e Papel na Arquitetura:
 * Este é o serviço mais crítico da aplicação. Ele garante a consistência relacional:
 * - Valida se um dispositivo físico (DeviceCode) existe, se está online e se não 
 *   está roubado/associado a outro usuário antes de vinculá-lo a uma planta.
 * - Gerencia o processo seguro de "desassociação", limpando as Foreign Keys no banco
 *   sem excluir o dispositivo físico da base de dados.
 * - Converte estruturas de banco (Aggregate Roots) em DTOs achatados para o Mobile.
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services;

public class PlantService : IPlantService
{
    private readonly IPlantRepository _plantRepo;
    private readonly IDeviceRepository _deviceRepo;
    private readonly ISpeciesRepository _speciesRepo;
    private readonly ILogger<PlantService> _logger;

    public PlantService(
        ILogger<PlantService> logger,
        IPlantRepository plantRepo,
        IDeviceRepository deviceRepo,
        ISpeciesRepository speciesRepo)
    {
        _logger = logger;

        _plantRepo = plantRepo;
        _deviceRepo = deviceRepo;
        _speciesRepo = speciesRepo;
    }

    // ── Listagem ──────────────────────────────────────────────────────────────

    public async Task<ServiceResult<IEnumerable<PlantSummaryResponse>>> GetAllAsync(Guid userId)
    {
        var plants = await _plantRepo.GetAllByUserAsync(userId);
        return ServiceResult<IEnumerable<PlantSummaryResponse>>.Ok(plants.Select(ToSummary));
    }

    // ── Detalhes ──────────────────────────────────────────────────────────────

    public async Task<ServiceResult<PlantDetailsResponse>> GetDetailsAsync(Guid plantId, Guid userId)
    {
        var plant = await _plantRepo.GetDetailsAsync(plantId, userId);
        if (plant is null)
            return ServiceResult<PlantDetailsResponse>.NotFound("Planta não encontrada.");

        return ServiceResult<PlantDetailsResponse>.Ok(ToDetails(plant));
    }

    // ── Criar ─────────────────────────────────────────────────────────────────

    // ── Helper compartilhado de validação de dispositivo ───────────────────────
    private async Task<(Device? Device, string? Error, int StatusCode)> ValidateDeviceForAssociationAsync(
        string deviceCode, Guid userId, Guid? currentPlantId)
    {
        var device = await _deviceRepo.GetByCodeAsync(deviceCode);
        if (device is null)
            return (null, "Código de dispositivo inválido.", 404);

        if (device.UserId is not null && device.UserId != userId)
            return (null, "Este dispositivo já está associado a outro usuário.", 409);

        // Se o dispositivo já pertence a ESTA planta, não é conflito (ex: reenvio do mesmo código)
        if (device.PlantId is not null && device.PlantId != currentPlantId)
            return (null, "Este dispositivo já está associado a uma planta.", 409);

        if (device.ConnectionStatus == ConnectionStatus.Offline)
            return (null, "Este dispositivo ainda não está conectado ao servidor.", 409);

        if (device.ConnectionStatus != ConnectionStatus.Online)
            return (null, "O dispositivo não está disponível para associação.", 409);

        return (device, null, 200);
    }

    public async Task<ServiceResult<PlantSummaryResponse>> CreateAsync(
    CreatePlantRequest request, Guid userId)
    {
        var species = await _speciesRepo.GetByIdAsync(request.SpeciesId);
        if (species is null)
            return ServiceResult<PlantSummaryResponse>.NotFound("Espécie não encontrada.");

        Device? device = null;
        if (!string.IsNullOrWhiteSpace(request.DeviceCode))
        {
            var (validDevice, error, status) = await ValidateDeviceForAssociationAsync(
                request.DeviceCode, userId, currentPlantId: null);

            if (error is not null)
                return ServiceResult<PlantSummaryResponse>.Fail(error, status);

            device = validDevice;
        }
        var plant = new Plant
        {
            UserId = userId,
            SpeciesId = species.Id,
            DeviceId = device?.Id,
            Name = request.Name,
            HealthStatus = HealthStatus.Healthy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _plantRepo.AddAsync(plant);

        if (device is not null)
        {
            device.PlantId = plant.Id;
            device.UserId = userId;
            device.ConnectionStatus = ConnectionStatus.Associated;
            await _deviceRepo.UpdateAsync(device);
        }

        var created = await _plantRepo.GetByIdAsync(plant.Id, userId);
        return ServiceResult<PlantSummaryResponse>.Ok(ToSummary(created!));
    }

    public async Task<ServiceResult> UpdateAsync(Guid plantId, UpdatePlantRequest request, Guid userId)
    {
        var plant = await _plantRepo.GetByIdAsync(plantId, userId);
        if (plant is null)
            return ServiceResult.NotFound("Planta não encontrada.");

        plant.Name = request.Name;

        if (request.DeviceCode is not null)
        {
            if (request.DeviceCode == string.Empty)
            {
                if (plant.DeviceId is not null)
                {
                    var oldDevice = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                    if (oldDevice is not null)
                    {
                        oldDevice.PlantId = null;
                        oldDevice.ConnectionStatus = ConnectionStatus.Online;
                        await _deviceRepo.UpdateAsync(oldDevice);
                    }
                    plant.DeviceId = null;
                }
            }
            else
            {
                var (newDevice, error, status) = await ValidateDeviceForAssociationAsync(
                    request.DeviceCode, userId, currentPlantId: plant.Id);

                if (error is not null)
                    return ServiceResult.Fail(error, status);

                // Desassocia o dispositivo antigo, se for diferente do novo
                if (plant.DeviceId is not null && plant.DeviceId != newDevice!.Id)
                {
                    var oldDevice = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                    if (oldDevice is not null)
                    {
                        oldDevice.PlantId = null;
                        oldDevice.ConnectionStatus = ConnectionStatus.Online;
                        await _deviceRepo.UpdateAsync(oldDevice);
                    }
                }

                plant.DeviceId = newDevice!.Id;
                newDevice.PlantId = plant.Id;
                newDevice.UserId = userId;
                newDevice.ConnectionStatus = ConnectionStatus.Associated;
                await _deviceRepo.UpdateAsync(newDevice);
            }
        }
        await _plantRepo.UpdateAsync(plant);
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AssociateDeviceAsync(
    Guid plantId, AssociateDeviceRequest request, Guid userId)
    {
        var plant = await _plantRepo.GetByIdAsync(plantId, userId);
        if (plant is null)
            return ServiceResult.NotFound("Planta não encontrada.");

        if (request.DeviceCode == string.Empty)
        {
            if (plant.DeviceId is not null)
            {
                var oldDevice = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                if (oldDevice is not null)
                {
                    oldDevice.PlantId = null;
                    oldDevice.ConnectionStatus = ConnectionStatus.Online;
                    await _deviceRepo.UpdateAsync(oldDevice);
                }
                plant.DeviceId = null;
            }
        }
        else
        {
            var (newDevice, error, status) = await ValidateDeviceForAssociationAsync(
                request.DeviceCode, userId, currentPlantId: plant.Id);

            if (error is not null)
                return ServiceResult.Fail(error, status);

            // Desassocia o antigo, se existir e for diferente do novo
            if (plant.DeviceId is not null && plant.DeviceId != newDevice!.Id)
            {
                var oldDevice = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                if (oldDevice is not null)
                {
                    oldDevice.PlantId = null;
                    oldDevice.ConnectionStatus = ConnectionStatus.Online;
                    await _deviceRepo.UpdateAsync(oldDevice);
                }
            }

            plant.DeviceId = newDevice!.Id;
            newDevice.PlantId = plant.Id;
            newDevice.UserId = userId;
            newDevice.ConnectionStatus = ConnectionStatus.Associated;
            await _deviceRepo.UpdateAsync(newDevice);
        }

        await _plantRepo.UpdateAsync(plant);
        return ServiceResult.Ok();
    }

    // ── Excluir ───────────────────────────────────────────────────────────────

    public async Task<ServiceResult> DeleteAsync(Guid plantId, Guid userId)
    {
        var plant = await _plantRepo.GetByIdAsync(plantId, userId);
        if (plant is null)
            return ServiceResult.NotFound("Planta não encontrada.");

        if (plant.DeviceId is not null)
        {
            var device = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
            if (device is not null)
            {
                device.PlantId = null;
                //device.UserId = null;
                device.ConnectionStatus = ConnectionStatus.Online;
                await _deviceRepo.UpdateAsync(device);
            }
        }
        await _plantRepo.DeleteAsync(plant);
        return ServiceResult.Ok();
    }

    // ── Mapeamentos ───────────────────────────────────────────────────────────

    private static PlantSummaryResponse ToSummary(Plant p) => new(
        Id: p.Id,
        Name: p.Name,
        Species: p.Species.Name,
        PhotoUrl: p.PhotoUrl ?? p.Species.PhotoUrl,
        HealthStatus: p.HealthStatus.ToString().ToUpper(),
        StatusReason: p.StatusReason);

    private static PlantDetailsResponse ToDetails(Plant p) => new(
        Id: p.Id,
        Name: p.Name,
        Species: p.Species.Name,
        PhotoUrl: p.PhotoUrl ?? p.Species.PhotoUrl,
        HealthStatus: p.HealthStatus.ToString().ToUpper(),
        StatusReason: p.StatusReason,
        Device: new DeviceStatusResponse(
            DeviceCode: p.Device?.Code,
            ConnectionStatus: (p.Device?.ConnectionStatus ?? ConnectionStatus.Unassociated)
                .ToString().ToUpper(),
            LastReadingAt: p.Device?.LastHeartbeatAt),
        Indicators: BuildIndicators(p),
        RecentNotifications: p.Notifications.Select(n => new NotificationSummaryResponse(
            Id: n.Id,
            PlantId: n.PlantId,
            PlantName: p.Name,
            Message: n.Message,
            Priority: n.Priority.ToString().ToUpper(),
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt)));

    private static IEnumerable<SensorIndicatorResponse> BuildIndicators(Plant p)
    {
        var readings24h = p.SensorReadings.ToList();
        var species = p.Species;

        var sensorConfigs = new[]
        {
            (Type: SensorType.SoilMoisture, Unit: "%",
             Min: species.SoilMoistureMin, Max: species.SoilMoistureMax),
            (Type: SensorType.AirHumidity,  Unit: "%",
             Min: species.AirHumidityMin,  Max: species.AirHumidityMax),
            (Type: SensorType.Temperature,  Unit: "°C",
             Min: species.TemperatureMin,  Max: species.TemperatureMax),
        };

        return sensorConfigs.Select(config =>
        {
            var typeReadings = readings24h
                .Where(r => r.SensorType == config.Type)
                .OrderBy(r => r.ReadAt)
                .ToList();

            var currentValue = typeReadings.LastOrDefault()?.Value ?? 0;
            var isWithin = currentValue >= config.Min && currentValue <= config.Max;

            return new SensorIndicatorResponse(
                SensorType: config.Type.ToString().ToUpper(),
                CurrentValue: (double)currentValue,
                Unit: config.Unit,
                IdealRange: new IdealRangeDto(config.Min, config.Max, config.Unit),
                IsWithinIdealRange: isWithin,
                RecentHistory: typeReadings.Select(r =>
                    new SensorReadingPointResponse(r.ReadAt, (double)r.Value)));
        });
    }
}