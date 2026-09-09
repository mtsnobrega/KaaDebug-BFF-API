/*
 * Responsabilidade:
 * Representa a entidade 'User' no banco de dados. Armazena informações de autenticação, 
 * preferências de notificação e os relacionamentos do usuário.
 *
 * Relacionamentos principais:
 * - 1:N com Plant (Um usuário possui várias plantas).
 * - 1:N com Notification (Um usuário possui várias notificações).
 *
 * Papel na arquitetura:
 * Atua na camada de Domínio/Persistência. É mapeada pelo Entity Framework Core para 
 * a tabela 'users' no PostgreSQL.
 */
namespace kaadebug_bff_api.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool NotificationsEnabled { get; set; } = true;
        public bool CriticalAlertsOnly { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public ICollection<Plant> Plants { get; set; } = new List<Plant>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}