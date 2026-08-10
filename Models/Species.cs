using System.Text.Json;

namespace kaadebug_bff_api.Models
{
    public class Species
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }

        // Faixas ideais — umidade do solo
        public decimal SoilMoistureMin { get; set; }
        public decimal SoilMoistureMax { get; set; }

        // Faixas ideais — umidade do ar
        public decimal AirHumidityMin { get; set; }
        public decimal AirHumidityMax { get; set; }

        // Faixas ideais — temperatura
        public decimal TemperatureMin { get; set; }
        public decimal TemperatureMax { get; set; }

        // Faixas ideais — luminosidade
        public decimal LuminosityMin { get; set; }
        public decimal LuminosityMax { get; set; }

        /// <summary>
        /// Dicas de cuidados armazenadas como JSONB no PostgreSQL.
        /// Mapeado como JsonDocument para permitir leitura/escrita flexível
        /// sem exigir uma estrutura rígida no banco.
        /// </summary>
        public JsonDocument? CareInfo { get; set; }

        // Navegação
        public ICollection<Plant> Plants { get; set; } = new List<Plant>();
    }
}
