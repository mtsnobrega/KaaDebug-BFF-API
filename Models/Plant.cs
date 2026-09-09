/*
 * Responsabilidade:
 * Representa a entidade principal do domínio, a 'Plant' (Planta pertencente ao usuário).
 * Age como a "Root Aggregate", vinculando o Usuário (Dono), a Espécie (Configurações base) 
 * e o Dispositivo IoT (Hardware que monitora).
 *
 * Relacionamentos:
 * - 1:N com User, Species
 * - 1:1 (Opcional) com Device (uma planta pode existir sem hardware acoplado).
 * - 1:N com SensorReadings, Notifications e DiagnosisResults.
 *
 * Papel na arquitetura:
 * Entidade central da aplicação (tabela 'plants'). A maior parte da lógica de negócio 
 * gira em torno do estado de saúde (HealthStatus) desta entidade.
 */

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


        public User User { get; set; } = null!;
        public Species Species { get; set; } = null!;
        public Device? Device { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<DiagnosisResult> DiagnosisResults { get; set; } = new List<DiagnosisResult>();
    }
}