/*
 * Responsabilidade:
 * Representa a entidade 'Species' (Espécie) no banco de dados, servindo como um 
 * catálogo mestre de plantas. Define os limites ideais de sensores (umidade, 
 * temperatura, luminosidade) para cada espécie.
 *
 * Tipo de dado notável:
 * Utiliza 'JsonDocument' para a propriedade 'CareInfo', que é persistida como 
 * JSONB no PostgreSQL, permitindo armazenar dicas de cuidados com esquema flexível.
 *
 * Papel na arquitetura:
 * Atua na camada de Domínio/Persistência (tabela 'species'). É utilizada para 
 * validar se as leituras de sensores das plantas estão dentro das faixas ideais.
 */
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
