namespace kaadebug_bff_api.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid PlantId { get; set; }
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Low;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        // Navegação
        public User User { get; set; } = null!;
        public Plant Plant { get; set; } = null!;
    }
}
