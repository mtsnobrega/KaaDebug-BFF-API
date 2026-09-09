/*
 * Responsabilidade:
 * Representa um hardware físico (ex: ESP32) responsável por coletar métricas do ambiente.
 * 
 * Regra de negócio explícita:
 * A entidade possui chaves para PlantId e UserId anuláveis (nullable). Isso 
 * significa que o banco foi modelado prevendo que um Dispositivo "nasce" 
 * na base de dados (registrado) sem dono, e só posteriormente é associado a um usuário/planta.
 *
 * Papel na arquitetura:
 * Domínio persistente (tabela 'devices').
 */

namespace kaadebug_bff_api.Models
{
    public class Device
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public ConnectionStatus ConnectionStatus { get; set; } = ConnectionStatus.Unassociated;
        public DateTime? LastHeartbeatAt { get; set; }
        public DateTime RegisteredAt { get; set; }

        // Define quem é o dono do dispositivo
        public Guid? PlantId { get; set; }
        public Guid? UserId { get; set; }

        public Plant? Plant { get; set; }
        public ICollection<SensorReading> SensorReadings { get; set; } = new List<SensorReading>();
    }
}