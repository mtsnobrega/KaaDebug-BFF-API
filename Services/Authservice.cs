using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace kaadebug_bff_api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        // Armazenamento em memória dos códigos OTP de recuperação.
        // Para produção: substituir por Redis ou tabela no banco com expiração.
        private static readonly Dictionary<string, (string Code, DateTime Expiry)> _recoveryCodes = new();

        public AuthService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return ServiceResult<LoginResponse>.Fail("E-mail ou senha incorretos.", 401);

            var (token, expiresAt) = GenerateJwt(user);

            return ServiceResult<LoginResponse>.Ok(new LoginResponse(
                Token: token,
                ExpiresAt: expiresAt,
                Name: user.Name,
                Email: user.Email));
        }

        public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepo.EmailExistsAsync(request.Email))
                return ServiceResult.Fail("Este e-mail já está cadastrado.", 409);

            var user = new User
            {
                Name = request.Name,
                Email = request.Email.ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> RequestPasswordRecoveryCodeAsync(
            RequestPasswordRecoveryCodeRequest request)
        {
            // Não informamos se o e-mail existe ou não — evita enumeração de usuários
            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user is not null)
            {
                var code = GenerateOtpCode();
                var expiry = DateTime.UtcNow.AddMinutes(15);

                _recoveryCodes[request.Email.ToLower()] = (code, expiry);

                // TODO: integrar com serviço de e-mail (SendGrid, SES, etc.)
                // await _emailService.SendRecoveryCodeAsync(request.Email, code);

                // Em desenvolvimento: loga o código para facilitar testes
                Console.WriteLine($"[DEV] Código de recuperação para {request.Email}: {code}");
            }

            return ServiceResult.Ok();
        }

        public Task<ServiceResult> ValidateRecoveryCodeAsync(ValidateRecoveryCodeRequest request)
        {
            var email = request.Email.ToLower();

            if (!_recoveryCodes.TryGetValue(email, out var entry))
                return Task.FromResult(ServiceResult.Fail("Código inválido ou expirado."));

            if (entry.Expiry < DateTime.UtcNow)
            {
                _recoveryCodes.Remove(email);
                return Task.FromResult(ServiceResult.Fail("Código expirado. Solicite um novo."));
            }

            if (entry.Code != request.Code)
                return Task.FromResult(ServiceResult.Fail("Código incorreto."));

            return Task.FromResult(ServiceResult.Ok());
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // Revalida o código no momento da troca — pode ter expirado entre as etapas
            var validationResult = await ValidateRecoveryCodeAsync(
                new ValidateRecoveryCodeRequest(request.Email, request.Code));

            if (!validationResult.Success)
                return validationResult;

            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user is null)
                return ServiceResult.Fail("Usuário não encontrado.", 404);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepo.UpdateAsync(user);

            _recoveryCodes.Remove(request.Email.ToLower());

            return ServiceResult.Ok();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private (string Token, DateTime ExpiresAt) GenerateJwt(User user)
        {
            var jwtKey = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key não configurado.");

            var expiresAt = DateTime.UtcNow.AddDays(
                int.TryParse(_config["Jwt:ExpirationDays"], out var days) ? days : 7);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        private static string GenerateOtpCode() =>
            Random.Shared.Next(100000, 999999).ToString();
    }
}
