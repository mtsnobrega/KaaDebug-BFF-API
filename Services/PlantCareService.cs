using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;
using System.Text.Json;

namespace kaadebug_bff_api.Services
{
    public class PlantCareService : IPlantCareService
    {
        private readonly IPlantRepository _plantRepo;

        public PlantCareService(IPlantRepository plantRepo)
        {
            _plantRepo = plantRepo;
        }

        public async Task<ServiceResult<object>> GetCareInfoAsync(Guid plantId, Guid userId)
        {
            var plant = await _plantRepo.GetByIdAsync(plantId, userId);

            if (plant is null)
                return ServiceResult<object>.NotFound("Planta não encontrada.");

            if (plant.Species.CareInfo is null)
                return ServiceResult<object>.Fail(
                    "Informações de cuidados não disponíveis para esta espécie.", 404);

            // Deserializa o JSONB para object para repassar diretamente ao cliente
            // sem precisar de uma classe tipada — o contrato do JSON é definido
            // no momento em que o dado é inserido no banco (pela equipe de conteúdo)
            var careInfo = JsonSerializer.Deserialize<object>(
                plant.Species.CareInfo.RootElement.GetRawText());

            return ServiceResult<object>.Ok(careInfo!);
        }
    }
}
