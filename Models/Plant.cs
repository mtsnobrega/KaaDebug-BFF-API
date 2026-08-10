namespace kaadebug_bff_api.Models
{
    public class Plant
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SpeciesId { get; set; }
        public Guid? DeviceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Healthy;
        public string? StatusReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navegação
        public User User { get; set; } = null!;
        public Species Species { get; set; } = null!;
        public Device? Device { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<DiagnosisResult> DiagnosisResults { get; set; } = new List<DiagnosisResult>();
    }
}
