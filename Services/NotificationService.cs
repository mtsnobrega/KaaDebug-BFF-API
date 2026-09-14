/*
 * Responsabilidade:
 * Processa a lógica de visualização e manipulação do histórico de 
 * notificações dos usuários.
 *
 * Papel na arquitetura:
 * Serviço enxuto que realiza operações de CRUD delegadas ao NotificationRepository,
 * garantindo o mapeamento de entidades para DTOs e limitando o impacto visual
 * de enums (transforma enum em string maiúscula para o frontend).
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepo;

        public NotificationService(INotificationRepository notificationRepo)
        {
            _notificationRepo = notificationRepo;
        }

        public async Task<ServiceResult<IEnumerable<NotificationSummaryResponse>>> GetAllAsync(Guid userId)
        {
            var notifications = await _notificationRepo.GetAllByUserAsync(userId);

            var response = notifications.Select(n => new NotificationSummaryResponse(
                Id: n.Id,
                PlantId: n.PlantId,
                PlantName: n.Plant.Name,
                Message: n.Message,
                Priority: n.Priority.ToString().ToUpper(),
                IsRead: n.IsRead,
                CreatedAt: n.CreatedAt));

            return ServiceResult<IEnumerable<NotificationSummaryResponse>>.Ok(response);
        }

        public async Task<ServiceResult> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _notificationRepo.GetByIdAsync(notificationId, userId);

            if (notification is null)
                return ServiceResult.NotFound("Notificação não encontrada.");

            if (notification.IsRead)
                return ServiceResult.Ok(); // já estava lida, sem necessidade de atualizar

            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification);

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> ClearAllAsync(Guid userId)
        {
            await _notificationRepo.DeleteAllByUserAsync(userId);
            return ServiceResult.Ok();
        }
    }
}
