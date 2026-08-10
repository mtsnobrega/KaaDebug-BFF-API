using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class SpeciesService : ISpeciesService
    {
        private readonly ISpeciesRepository _speciesRepo;

        public SpeciesService(ISpeciesRepository speciesRepo)
        {
            _speciesRepo = speciesRepo;
        }

        public async Task<ServiceResult<IEnumerable<SpeciesResponse>>> GetAllAsync()
        {
            var species = await _speciesRepo.GetAllAsync();

            var response = species.Select(s => new SpeciesResponse(
                Id: s.Id,
                Name: s.Name,
                PhotoUrl: s.PhotoUrl,
                SoilMoisture: new IdealRangeDto(s.SoilMoistureMin, s.SoilMoistureMax, "%"),
                AirHumidity: new IdealRangeDto(s.AirHumidityMin, s.AirHumidityMax, "%"),
                Temperature: new IdealRangeDto(s.TemperatureMin, s.TemperatureMax, "°C"),
                Luminosity: new IdealRangeDto(s.LuminosityMin, s.LuminosityMax, "lux")));

            return ServiceResult<IEnumerable<SpeciesResponse>>.Ok(response);
        }
    }
}
