/*
 * Responsabilidade:
 * Monta o painel principal (Dashboard) do aplicativo Mobile do usuário logado.
 *
 * Papel na arquitetura:
 * Demonstra a essência de um BFF (Backend for Frontend). Ele agrega chamadas de
 * múltiplos repositórios (Plant, Notification, User) e processa os dados no backend
 * para retornar um único payload enxuto (`DashboardResponse`), otimizando o tráfego 
 * de rede e simplificando o frontend mobile.
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IPlantRepository _plantRepo;
        private readonly INotificationRepository _notificationRepo;
        private readonly IUserRepository _userRepo;

        public DashboardService(
            IPlantRepository plantRepo,
            INotificationRepository notificationRepo,
            IUserRepository userRepo)
        {
            _plantRepo = plantRepo;
            _notificationRepo = notificationRepo;
            _userRepo = userRepo;
        }

        public async Task<ServiceResult<DashboardResponse>> GetDashboardAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);
            if (user is null)
                return ServiceResult<DashboardResponse>.NotFound("Usuário não encontrado.");

            var totalPlants = await _plantRepo.CountByUserAsync(userId);
            var activeAlerts = await _notificationRepo.CountUnreadByUserAsync(userId);
            var recentPlants = await _plantRepo.GetAllByUserAsync(userId);
            var recentNotifs = await _notificationRepo.GetRecentUnreadByUserAsync(userId, limit: 3);

            var response = new DashboardResponse(
                UserFirstName: user.Name.Split(' ')[0],
                TotalPlants: totalPlants,
                ActiveAlertsCount: activeAlerts,
                RecentPlants: recentPlants.Take(3).Select(p => new PlantSummaryResponse(
                    Id: p.Id,
                    Name: p.Name,
                    Species: p.Species.Name,
                    PhotoUrl: p.PhotoUrl ?? p.Species.PhotoUrl,
                    HealthStatus: p.HealthStatus.ToString().ToUpper(),
                    StatusReason: p.StatusReason)),
                RecentNotifications: recentNotifs.Select(n => new NotificationSummaryResponse(
                    Id: n.Id,
                    PlantId: n.PlantId,
                    PlantName: n.Plant.Name,
                    Message: n.Message,
                    Priority: n.Priority.ToString().ToUpper(),
                    IsRead: n.IsRead,
                    CreatedAt: n.CreatedAt)));

            return ServiceResult<DashboardResponse>.Ok(response);
        }
    }
}