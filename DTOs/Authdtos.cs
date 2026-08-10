using System.ComponentModel.DataAnnotations;

namespace kaadebug_bff_api.DTOs
{
    // ── Requests ──────────────────────────────────────────────────────────────────

    public record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(6)] string Password);

    public record RegisterRequest(
        [Required, MinLength(3), MaxLength(150)] string Name,
        [Required, EmailAddress, MaxLength(250)] string Email,
        [Required, MinLength(6)] string Password);

    public record RequestPasswordRecoveryCodeRequest(
        [Required, EmailAddress] string Email);

    public record ValidateRecoveryCodeRequest(
        [Required, EmailAddress] string Email,
        [Required, Length(6, 6)] string Code);

    public record ResetPasswordRequest(
        [Required, EmailAddress] string Email,
        [Required, Length(6, 6)] string Code,
        [Required, MinLength(6)] string NewPassword);

    // ── Responses ─────────────────────────────────────────────────────────────────

    public record LoginResponse(
        string Token,
        DateTime ExpiresAt,
        string Name,
        string Email);
}
