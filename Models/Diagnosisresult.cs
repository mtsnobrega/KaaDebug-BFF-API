/*
 * Responsabilidade:
 * Armazena os resultados de diagnósticos realizados sobre imagens enviadas 
 * pelos usuários para identificar doenças nas plantas.
 *
 * Flexibilidade:
 * O campo 'IssuesJson' (JsonDocument) é crucial, pois permite armazenar 
 * respostas dinâmicas de uma IA externa (com arrays de recomendações e níveis 
 * de confiança) sem precisar criar múltiplas tabelas relacionais para os detalhes.
 *
 * Papel na arquitetura:
 * Domínio persistente (tabela 'diagnosis_results').
 */

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