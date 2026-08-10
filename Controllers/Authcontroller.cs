using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace kaadebug_bff_api.Controllers
{
    [Route("auth")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Autentica o usuário e retorna o token JWT.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return ToActionResult(result);
        }

        /// <summary>
        /// Cadastra um novo usuário.
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return ToActionResult(result);
        }

        /// <summary>
        /// Solicita o envio do código OTP de recuperação para o e-mail informado.
        /// Sempre retorna 200 independente de o e-mail existir ou não
        /// (evita enumeração de usuários).
        /// </summary>
        [HttpPost("recovery/request-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RequestRecoveryCode(
            [FromBody] RequestPasswordRecoveryCodeRequest request)
        {
            await _authService.RequestPasswordRecoveryCodeAsync(request);
            return Ok();
        }

        /// <summary>
        /// Valida o código OTP recebido por e-mail.
        /// </summary>
        [HttpPost("recovery/validate-code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateRecoveryCode(
            [FromBody] ValidateRecoveryCodeRequest request)
        {
            var result = await _authService.ValidateRecoveryCodeAsync(request);
            return ToActionResult(result);
        }

        /// <summary>
        /// Redefine a senha usando o código OTP válido.
        /// </summary>
        [HttpPost("recovery/reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword(
            [FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return ToActionResult(result);
        }
    }
}
