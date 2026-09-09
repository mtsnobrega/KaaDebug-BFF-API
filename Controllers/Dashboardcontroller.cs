/*
 * Responsabilidade:
 * Ponto de entrada (endpoints) para o carregamento do Dashboard mobile.
 * 
 * Endpoints:
 * - GET /dashboard (Retorna agregados do sistema e resumos do usuário)
 * 
 * Serviço utilizado: IDashboardService
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Authorize]
    [Route("dashboard")]
    public class DashboardController : ApiControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Retorna o resumo consolidado do usuário: plantas recentes,
        /// alertas ativos e notificações não lidas.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboard()
        {
            var result = await _dashboardService.GetDashboardAsync(CurrentUserId);
            return ToActionResult(result);
        }
    }
}
