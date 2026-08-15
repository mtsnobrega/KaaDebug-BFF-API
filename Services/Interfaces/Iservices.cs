using kaadebug_bff_api.DTOs;

namespace kaadebug_bff_api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ServiceResult> RegisterAsync(RegisterRequest request);
        Task<ServiceResult> RequestPasswordRecoveryCodeAsync(RequestPasswordRecoveryCodeRequest request);
        Task<ServiceResult> ValidateRecoveryCodeAsync(ValidateRecoveryCodeRequest request);
        Task<ServiceResult> ResetPasswordAsync(ResetPasswordRequest request);
    }

    public interface IDashboardService
    {
        Task<ServiceResult<DashboardResponse>> GetDashboardAsync(Guid userId);
    }

    public interface ISpeciesService
    {
        Task<ServiceResult<IEnumerable<SpeciesResponse>>> GetAllAsync();
    }

    public interface IPlantService
    {
        Task<ServiceResult<IEnumerable<PlantSummaryResponse>>> GetAllAsync(Guid userId);
        Task<ServiceResult<PlantDetailsResponse>> GetDetailsAsync(Guid plantId, Guid userId);
        Task<ServiceResult<PlantSummaryResponse>> CreateAsync(CreatePlantRequest request, Guid userId);
        Task<ServiceResult> UpdateAsync(Guid plantId, UpdatePlantRequest request, Guid userId);
        Task<ServiceResult> DeleteAsync(Guid plantId, Guid userId);


        Task<ServiceResult> AssociateDeviceAsync(Guid plantId, AssociateDeviceRequest request, Guid userId);
    }

    public interface IPlantHistoryService
    {
        Task<ServiceResult<PlantHistoryResponse>> GetHistoryAsync(Guid plantId, Guid userId, string period);
    }

    public interface IDeviceService
    {
        Task<ServiceResult<DeviceVerificationResponse>> VerifyAsync(string deviceCode);
    }

    public interface INotificationService
    {
        Task<ServiceResult<IEnumerable<NotificationSummaryResponse>>> GetAllAsync(Guid userId);
        Task<ServiceResult> MarkAsReadAsync(Guid notificationId, Guid userId);
        Task<ServiceResult> ClearAllAsync(Guid userId);
    }

    public interface IDiagnosisService
    {
        Task<ServiceResult<DiagnosisResultResponse>> AnalyzeAsync(Guid plantId, Guid userId, Stream imageStream, string fileName);
        Task<ServiceResult<IEnumerable<DiagnosisResultResponse>>> GetHistoryAsync(Guid plantId, Guid userId);
    }

    public interface IPlantCareService
    {
        Task<ServiceResult<object>> GetCareInfoAsync(Guid plantId, Guid userId);
    }

    public interface IProfileService
    {
        Task<ServiceResult<ProfileResponse>> GetAsync(Guid userId);
        Task<ServiceResult> UpdateAsync(Guid userId, UpdateProfileRequest request);
        Task<ServiceResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    }

    // ── Result pattern ────────────────────────────────────────────────────────────

    /// <summary>
    /// Resultado genérico de operações de serviço.
    /// Evita exceptions para erros de negócio esperados (ex: e-mail já cadastrado,
    /// planta não encontrada). Exceptions ficam reservadas para erros técnicos.
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; protected init; }
        public string? ErrorMessage { get; protected init; }
        public int StatusCode { get; protected init; } = 200;

        public static ServiceResult Ok() => new() { Success = true };

        public static ServiceResult Fail(string message, int statusCode = 400) =>
            new() { Success = false, ErrorMessage = message, StatusCode = statusCode };

        public static ServiceResult NotFound(string message = "Recurso não encontrado.") =>
            new() { Success = false, ErrorMessage = message, StatusCode = 404 };

        public static ServiceResult Unauthorized(string message = "Não autorizado.") =>
            new() { Success = false, ErrorMessage = message, StatusCode = 401 };
    }

    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; private init; }

        public static ServiceResult<T> Ok(T data) =>
            new() { Success = true, Data = data };

        public new static ServiceResult<T> Fail(string message, int statusCode = 400) =>
            new() { Success = false, ErrorMessage = message, StatusCode = statusCode };

        public new static ServiceResult<T> NotFound(string message = "Recurso não encontrado.") =>
            new() { Success = false, ErrorMessage = message, StatusCode = 404 };
    }




}
