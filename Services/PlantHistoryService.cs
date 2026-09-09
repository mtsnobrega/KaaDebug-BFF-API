/*
 * Responsabilidade:
 * Agrega e calcula métricas do histórico de sensores (SensorReadings) de uma planta.
 *
 * Papel na arquitetura:
 * Regra de negócio focada em UX. A API processa os cálculos matemáticos (Mínimo, 
 * Máximo, Média) das leituras agrupadas por tipo de sensor. O BFF alivia o 
 * aplicativo Mobile de realizar processamento iterativo pesado.
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class PlantHistoryService : IPlantHistoryService
    {
        private readonly IPlantRepository _plantRepo;
        private readonly ISensorReadingRepository _sensorRepo;

        public PlantHistoryService(
            IPlantRepository plantRepo,
            ISensorReadingRepository sensorRepo)
        {
            _plantRepo = plantRepo;
            _sensorRepo = sensorRepo;
        }

        public async Task<ServiceResult<PlantHistoryResponse>> GetHistoryAsync(
            Guid plantId, Guid userId, string period)
        {
            var plant = await _plantRepo.GetByIdAsync(plantId, userId);
            if (plant is null)
                return ServiceResult<PlantHistoryResponse>.NotFound("Planta não encontrada.");

            var (from, periodLabel) = ResolvePeriod(period);
            var readings = await _sensorRepo.GetHistoryAsync(plantId, from, DateTime.UtcNow);

            var sensorConfigs = new[]
            {
            (Type: SensorType.SoilMoisture, Unit: "%",
             Min: plant.Species.SoilMoistureMin, Max: plant.Species.SoilMoistureMax),
            (Type: SensorType.AirHumidity,  Unit: "%",
             Min: plant.Species.AirHumidityMin,  Max: plant.Species.AirHumidityMax),
            (Type: SensorType.Temperature,  Unit: "°C",
             Min: plant.Species.TemperatureMin,  Max: plant.Species.TemperatureMax),
        };
            var sensors = sensorConfigs.Select(config =>
            {
                var typeReadings = readings
                    .Where(r => r.SensorType == config.Type)
                    .OrderBy(r => r.ReadAt)
                    .ToList();

                var values = typeReadings.Select(r => (double)r.Value).ToList();

                return new SensorHistoryResponse(
                    SensorType: config.Type.ToString().ToUpper(),
                    Unit: config.Unit,
                    IdealRange: new IdealRangeDto(config.Min, config.Max, config.Unit),
                    MinValue: values.Count > 0 ? values.Min() : null,
                    MaxValue: values.Count > 0 ? values.Max() : null,
                    AvgValue: values.Count > 0 ? values.Average() : null,
                    Readings: typeReadings.Select(r =>
                        new SensorReadingPointResponse(r.ReadAt, (double)r.Value)));
            });

            var response = new PlantHistoryResponse(
                PlantName: plant.Name,
                Period: periodLabel,
                Sensors: sensors);

            return ServiceResult<PlantHistoryResponse>.Ok(response);
        }

        /// <summary>
        /// Converte a string do query param (24h, 7d, 30d) em DateTime de corte.
        /// Valor inválido ou não informado usa 24h como padrão.
        /// </summary>
        private static (DateTime From, string Label) ResolvePeriod(string period) =>
            period?.ToLower() switch
            {
                "7d" => (DateTime.UtcNow.AddDays(-7), "7d"),
                "30d" => (DateTime.UtcNow.AddDays(-30), "30d"),
                _ => (DateTime.UtcNow.AddHours(-24), "24h")
            };
    }
}