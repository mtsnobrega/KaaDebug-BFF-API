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

        // Navegação
        public ICollection<Plant> Plants { get; set; } = new List<Plant>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
