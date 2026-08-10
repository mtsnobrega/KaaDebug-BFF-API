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

        // Navegação
        public Plant Plant { get; set; } = null!;
        public Device Device { get; set; } = null!;
    }
}
