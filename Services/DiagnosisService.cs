using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;
using System.Text.Json;

namespace kaadebug_bff_api.Services
{
    public class DiagnosisService : IDiagnosisService
    {
        private readonly IDiagnosisRepository _diagnosisRepo;
        private readonly IPlantRepository _plantRepo;

        // TODO: injetar serviço de storage (Azure Blob / S3) quando disponível
        // private readonly IStorageService _storageService;

        public DiagnosisService(
            IDiagnosisRepository diagnosisRepo,
            IPlantRepository plantRepo)
        {
            _diagnosisRepo = diagnosisRepo;
            _plantRepo = plantRepo;
        }

        public async Task<ServiceResult<DiagnosisResultResponse>> AnalyzeAsync(
            Guid plantId, Guid userId, Stream imageStream, string fileName)
        {
            var plant = await _plantRepo.GetByIdAsync(plantId, userId);
            if (plant is null)
                return ServiceResult<DiagnosisResultResponse>.NotFound("Planta não encontrada.");

            // TODO: fazer upload da imagem para storage externo e obter a URL real
            // var imageUrl = await _storageService.UploadAsync(imageStream, fileName);
            var imageUrl = $"uploads/{Guid.NewGuid()}_{fileName}"; // placeholder

            // TODO: chamar serviço de IA para análise da imagem
            // var aiResult = await _aiService.AnalyzeAsync(imageStream);
            // Por hora retorna um diagnóstico placeholder até o serviço de IA ser integrado
            var issues = new List<DiagnosisIssueResponse>();
            var isHealthy = true;
            var observation = "Análise pendente — serviço de IA não configurado.";

            var issuesJson = JsonDocument.Parse(JsonSerializer.Serialize(issues));

            var diagnosis = new DiagnosisResult
            {
                PlantId = plantId,
                ImageUrl = imageUrl,
                IsHealthy = isHealthy,
                OverallObservation = observation,
                IssuesJson = issuesJson,
                PerformedAt = DateTime.UtcNow
            };

            await _diagnosisRepo.AddAsync(diagnosis);

            return ServiceResult<DiagnosisResultResponse>.Ok(ToResponse(diagnosis, issues));
        }

        public async Task<ServiceResult<IEnumerable<DiagnosisResultResponse>>> GetHistoryAsync(
            Guid plantId, Guid userId)
        {
            var plant = await _plantRepo.GetByIdAsync(plantId, userId);
            if (plant is null)
                return ServiceResult<IEnumerable<DiagnosisResultResponse>>.NotFound(
                    "Planta não encontrada.");

            var results = await _diagnosisRepo.GetByPlantAsync(plantId, userId);

            var response = results.Select(d =>
            {
                var issues = ParseIssues(d.IssuesJson);
                return ToResponse(d, issues);
            });

            return ServiceResult<IEnumerable<DiagnosisResultResponse>>.Ok(response);
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static DiagnosisResultResponse ToResponse(
            DiagnosisResult d,
            IEnumerable<DiagnosisIssueResponse> issues) =>
            new(
                Id: d.Id,
                IsHealthy: d.IsHealthy,
                OverallObservation: d.OverallObservation,
                Issues: issues,
                PerformedAt: d.PerformedAt);

        private static IEnumerable<DiagnosisIssueResponse> ParseIssues(JsonDocument? json)
        {
            if (json is null)
                return Enumerable.Empty<DiagnosisIssueResponse>();

            try
            {
                return JsonSerializer.Deserialize<IEnumerable<DiagnosisIssueResponse>>(
                    json.RootElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? Enumerable.Empty<DiagnosisIssueResponse>();
            }
            catch
            {
                return Enumerable.Empty<DiagnosisIssueResponse>();
            }
        }
    }
}
