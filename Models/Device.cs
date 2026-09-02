namespace kaadebug_bff_api.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Unassociated;
        public DateTime? LastHeartbeatAt { get; set; }
        public DateTime RegisteredAt { get; set; }

        // NOVA PROPRIEDADE: Define quem é o dono! 
        // Pode ser nula porque ele nasce livre na fábrica.
        public Guid? PlantId { get; set; }
        public Guid? UserId { get; set; }

        // Navegação
        public Plant? Plant { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    }
}
