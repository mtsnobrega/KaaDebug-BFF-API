using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepo;
        private readonly IDeviceRepository _deviceRepo;
        private readonly ISpeciesRepository _speciesRepo;

        public PlantService(
            IPlantRepository plantRepo,
            IDeviceRepository deviceRepo,
            ISpeciesRepository speciesRepo)
        {
            _plantRepo = plantRepo;
            _deviceRepo = deviceRepo;
            _speciesRepo = speciesRepo;
        }

        public async Task<ServiceResult<IEnumerable<PlantSummaryResponse>>> GetAllAsync(Guid userId)
        {
            var plants = await _plantRepo.GetAllByUserAsync(userId);
            return ServiceResult<IEnumerable<PlantSummaryResponse>>.Ok(
                plants.Select(ToSummary));
        }

        public async Task<ServiceResult<PlantDetailsResponse>> GetDetailsAsync(Guid plantId, Guid userId)
        {
            var plant = await _plantRepo.GetDetailsAsync(plantId, userId);

            if (plant is null)
                return ServiceResult<PlantDetailsResponse>.NotFound("Planta não encontrada.");

            return ServiceResult<PlantDetailsResponse>.Ok(ToDetails(plant));
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
                device = await _deviceRepo.GetByCodeAsync(request.DeviceCode);
                if (device is null)
                    return ServiceResult<PlantSummaryResponse>.Fail("Código de dispositivo inválido.", 404);
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

            if (device is not null)
                device.ConnectionStatus = ConnectionStatus.Online;

            await _plantRepo.AddAsync(plant);

            // Carrega a planta completa para retornar com Species e Device populados
            var created = await _plantRepo.GetByIdAsync(plant.Id, userId);
            return ServiceResult<PlantSummaryResponse>.Ok(ToSummary(created!));
        }

        public async Task<ServiceResult> UpdateAsync(Guid plantId, UpdatePlantRequest request, Guid userId)
        {
            var plant = await _plantRepo.GetByIdAsync(plantId, userId);
            if (plant is null)
                return ServiceResult.NotFound("Planta não encontrada.");

            plant.Name = request.Name;

            // Gestão de dispositivo: 3 casos conforme definido no contrato do DTO
            if (request.DeviceCode is not null)
            {
                if (request.DeviceCode == string.Empty)
                {
                    // Desassociar dispositivo atual
                    if (plant.DeviceId is not null)
                    {
                        var oldDevice = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                        if (oldDevice is not null)
                        {
                            oldDevice.ConnectionStatus = ConnectionStatus.Unassociated;
                            await _deviceRepo.UpdateAsync(oldDevice);
                        }
                        plant.DeviceId = null;
                    }
                }
                else
                {
                    // Associar/substituir dispositivo
                    var newDevice = await _deviceRepo.GetByCodeAsync(request.DeviceCode);
                    if (newDevice is null)
                        return ServiceResult.Fail("Código de dispositivo inválido.", 404);

                    // Se havia dispositivo anterior, libera ele
                    if (plant.DeviceId is not null && plant.DeviceId != newDevice.Id)
                    {
                        var oldDevice = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                        if (oldDevice is not null)
                        {
                            oldDevice.ConnectionStatus = ConnectionStatus.Unassociated;
                            await _deviceRepo.UpdateAsync(oldDevice);
                        }
                    }

                    plant.DeviceId = newDevice.Id;
                }
            }

            await _plantRepo.UpdateAsync(plant);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> DeleteAsync(Guid plantId, Guid userId)
        {
            var plant = await _plantRepo.GetByIdAsync(plantId, userId);
            if (plant is null)
                return ServiceResult.NotFound("Planta não encontrada.");

            // Libera o dispositivo antes de deletar a planta
            if (plant.DeviceId is not null)
            {
                var device = await _deviceRepo.GetByIdAsync(plant.DeviceId.Value);
                if (device is not null)
                {
                    device.ConnectionStatus = ConnectionStatus.Unassociated;
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
                ConnectionStatus: (p.Device?.ConnectionStatus ?? ConnectionStatus.Unassociated).ToString().ToUpper(),
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
            (Type: SensorType.Luminosity,   Unit: "lux",
             Min: species.LuminosityMin,   Max: species.LuminosityMax),
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
}
