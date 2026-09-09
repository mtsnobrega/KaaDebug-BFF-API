/*
 * Responsabilidade:
 * Ponto de entrada (endpoints) para controle da caixa de alertas do usuário.
 * 
 * Endpoints:
 * - GET /notifications (Lista todas as notificações, não lidas no topo)
 * - PUT /notifications/{id}/read (Marca notificação específica como lida)
 * - DELETE /notifications (Apaga todo o histórico)
 * 
 * Serviço utilizado: INotificationService
 */
using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Authorize]
    [Route("notifications")]
    public class NotificationsController : ApiControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Lista todas as notificações do usuário autenticado,
        /// ordenadas por não lidas primeiro e depois por data decrescente.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<NotificationSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _notificationService.GetAllAsync(CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Marca uma notificação como lida.
        /// Idempotente: se já estava lida, retorna 200 sem erro.
        /// </summary>
        [HttpPut("{id:guid}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead([FromRoute] Guid id)
        {
            var result = await _notificationService.MarkAsReadAsync(id, CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Remove todas as notificações do usuário autenticado.
        /// </summary>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearAll()
        {
            var result = await _notificationService.ClearAllAsync(CurrentUserId);
            return ToActionResult(result);
        }
    }
}
