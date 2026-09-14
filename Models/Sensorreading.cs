/*
 * Responsabilidade:
 * Representa a entidade 'SensorReading', que armazena os dados históricos (time-series)
 * captados pelos dispositivos (ESP32) acoplados às plantas.
 *
 * Otimização:
 * Utiliza o tipo long (BIGINT) para o Id em vez de Guid, gerado automaticamente como 
 * IDENTITY, o que melhora significativamente a performance de inserção massiva e 
 * indexação para tabelas de série temporal.
 *
 * Papel na arquitetura:
 * Tabela transacional pesada. Usada para gerar os gráficos de histórico do Dashboard.
 */

namespace kaadebug_bff_api.Models
{
    public class SensorReading
    {
        /// <summary>
        /// BIGINT GENERATED ALWAYS AS IDENTITY no PostgreSQL.
        /// O EF Core é configurado com ValueGeneratedOnAdd no DbContext.
        /// </summary>
        public long Id { get; set; }

        public Guid PlantId { get; set; }
        public Guid DeviceId { get; set; }
        public SensorType SensorType { get; set; }
        public decimal Value { get; set; }
        public bool IsWithinIdealRange { get; set; }
        public DateTime ReadAt { get; set; }

        public Plant Plant { get; set; } = null!;
        public Device Device { get; set; } = null!;
    }
}
