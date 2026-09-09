/*
 * Responsabilidade:
 * Ponto de entrada (endpoints) para as operações gerenciais da conta do usuário.
 * 
 * Endpoints:
 * - GET /profile (Retorna informações do perfil atual)
 * - PUT /profile (Atualiza chaves públicas: nome e opções de notificação)
 * - POST /profile/change-password (Reescreve hash com segurança)
 * 
 * Serviço utilizado: IProfileService
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Authorize]
    [Route("profile")]
    public class ProfileController : ApiControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// Retorna os dados do perfil do usuário autenticado.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var result = await _profileService.GetAsync(CurrentUserId);
            return ToActionResult(result);
        }

        /// <summary>
        /// Atualiza nome e preferências de notificação do usuário.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update([FromBody] UpdateProfileRequest request)
        {
            var result = await _profileService.UpdateAsync(CurrentUserId, request);
            return ToActionResult(result);
        }

        /// <summary>
        /// Altera a senha do usuário com validação da senha atual.
        /// </summary>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var result = await _profileService.ChangePasswordAsync(CurrentUserId, request);
            return ToActionResult(result);
        }
    }
}
