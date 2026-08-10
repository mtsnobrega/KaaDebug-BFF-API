using System.Text.Json;

namespace kaadebug_bff_api.Models
{
    public class DiagnosisResult
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsHealthy { get; set; }
        public string? OverallObservation { get; set; }

        /// <summary>
        /// Lista de problemas identificados pela IA, armazenada como JSONB.
        /// Estrutura esperada: array de objetos com name, confidencePercent,
        /// description e recommendations (array de strings).
        /// </summary>
        public JsonDocument? IssuesJson { get; set; }

        public DateTime PerformedAt { get; set; }

        // Navegação
        public Plant Plant { get; set; } = null!;
    }
}
