/*
 * Responsabilidade:
 * Expõe a listagem e leitura de dados das Espécies de plantas suportadas pelo sistema.
 *
 * Papel na arquitetura:
 * Tradução de catálogo (Entidade de Domínio -> DTO). Fornece dados essenciais 
 * que alimentam, por exemplo, os comboboxes/listas de seleção no momento do cadastro 
 * de novas plantas no aplicativo Mobile.
 */

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
                Temperature: new IdealRangeDto(s.TemperatureMin, s.TemperatureMax, "°C")));

            return ServiceResult<IEnumerable<SpeciesResponse>>.Ok(response);
        }
    }
}