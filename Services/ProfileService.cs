/*
 * Responsabilidade:
 * Gerencia a atualização de informações cadastrais e redefinição de senha do 
 * usuário logado.
 *
 * Papel na arquitetura:
 * Atua como mediador seguro para a entidade User. Impede, por exemplo, que 
 * campos sensíveis sejam sobrescritos inadvertidamente, separando o que pode ser 
 * alterado (Nome, Configurações de Notificações) através da injeção do BCrypt na 
 * troca de senha.
 */

using kaadebug_bff_api.DTOs;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services.Interfaces;

namespace kaadebug_bff_api.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepo;

        public ProfileService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<ServiceResult<ProfileResponse>> GetAsync(Guid userId)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return ServiceResult<ProfileResponse>.NotFound("Usuário não encontrado.");

            return ServiceResult<ProfileResponse>.Ok(new ProfileResponse(
                Name: user.Name,
                Email: user.Email,
                NotificationsEnabled: user.NotificationsEnabled,
                CriticalAlertsOnly: user.CriticalAlertsOnly));
        }

        public async Task<ServiceResult> UpdateAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return ServiceResult.NotFound("Usuário não encontrado.");

            user.Name = request.Name;
            user.NotificationsEnabled = request.NotificationsEnabled;
            user.CriticalAlertsOnly = request.CriticalAlertsOnly;

            await _userRepo.UpdateAsync(user);
            return ServiceResult.Ok();
        }

        public async Task<ServiceResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _userRepo.GetByIdAsync(userId);

            if (user is null)
                return ServiceResult.NotFound("Usuário não encontrado.");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return ServiceResult.Fail("Senha atual incorreta.", 400);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _userRepo.UpdateAsync(user);

            return ServiceResult.Ok();
        }
    }
}