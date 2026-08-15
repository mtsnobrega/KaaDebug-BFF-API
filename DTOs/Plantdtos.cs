using System.ComponentModel.DataAnnotations;

namespace kaadebug_bff_api.DTOs
{
    // ── Species ───────────────────────────────────────────────────────────────────

    public record SpeciesResponse(
        Guid Id,
        string Name,
        string? PhotoUrl,
        IdealRangeDto SoilMoisture,
        IdealRangeDto AirHumidity,
        IdealRangeDto Temperature,
        IdealRangeDto Luminosity);

    public record IdealRangeDto(decimal Min, decimal Max, string Unit);

    // ── Plants ────────────────────────────────────────────────────────────────────

    public record CreatePlantRequest(
        [Required, MinLength(2), MaxLength(150)] string Name,
        [Required] Guid SpeciesId,
        string? DeviceCode);

    public record UpdatePlantRequest(
        [Required, MinLength(2), MaxLength(150)] string Name,

        /// <summary>
        /// null  = não altera o dispositivo atual
        /// ""    = desassocia o dispositivo atual
        /// "ESP32-XXXX" = associa/substitui pelo novo dispositivo
        /// </summary>
        string? DeviceCode);

    public record PlantSummaryResponse(
        Guid Id,
        string Name,
        string Species,
        string? PhotoUrl,
        string HealthStatus,
        string? StatusReason);

    public record PlantDetailsResponse(
        Guid Id,
        string Name,
        string Species,
        string? PhotoUrl,
        string HealthStatus,
        string? StatusReason,
        DeviceStatusResponse Device,
        IEnumerable<SensorIndicatorResponse> Indicators,
        IEnumerable<NotificationSummaryResponse> RecentNotifications);

    public record DeviceStatusResponse(
        string? DeviceCode,
        string ConnectionStatus,
        DateTime? LastReadingAt);

    public record SensorIndicatorResponse(
        string SensorType,
        double CurrentValue,
        string Unit,
        IdealRangeDto IdealRange,
        bool IsWithinIdealRange,
        IEnumerable<SensorReadingPointResponse> RecentHistory);

    public record SensorReadingPointResponse(DateTime Timestamp, double Value);

    // ── History ───────────────────────────────────────────────────────────────────

    public record PlantHistoryResponse(
        string PlantName,
        string Period,
        IEnumerable<SensorHistoryResponse> Sensors);

    public record SensorHistoryResponse(
        string SensorType,
        string Unit,
        IdealRangeDto IdealRange,
        double? MinValue,
        double? MaxValue,
        double? AvgValue,
        IEnumerable<SensorReadingPointResponse> Readings);

    // ── Dashboard ─────────────────────────────────────────────────────────────────

    public record DashboardResponse(
        string UserFirstName,
        int TotalPlants,
        int ActiveAlertsCount,
        IEnumerable<PlantSummaryResponse> RecentPlants,
        IEnumerable<NotificationSummaryResponse> RecentNotifications);

    // ── Diagnosis ─────────────────────────────────────────────────────────────────

    public record DiagnosisResultResponse(
        Guid Id,
        bool IsHealthy,
        string? OverallObservation,
        IEnumerable<DiagnosisIssueResponse> Issues,
        DateTime PerformedAt);

    public record DiagnosisIssueResponse(
        string Name,
        int ConfidencePercent,
        string Description,
        IEnumerable<string> Recommendations);

    // ── Notifications ─────────────────────────────────────────────────────────────

    public record NotificationSummaryResponse(
        Guid Id,
        Guid PlantId,
        string PlantName,
        string Message,
        string Priority,
        bool IsRead,
        DateTime CreatedAt);

    // ── Profile ───────────────────────────────────────────────────────────────────

    public record ProfileResponse(
        string Name,
        string Email,
        bool NotificationsEnabled,
        bool CriticalAlertsOnly);

    public record UpdateProfileRequest(
        [Required, MinLength(3), MaxLength(150)] string Name,
        bool NotificationsEnabled,
        bool CriticalAlertsOnly);

    public record ChangePasswordRequest(
        [Required] string CurrentPassword,
        [Required, MinLength(6)] string NewPassword);

    // ── Device ────────────────────────────────────────────────────────────────────

    public record DeviceVerificationResponse(
        string Code,
        string ConnectionStatus,
        DateTime? LastHeartbeatAt);

    public record AssociateDeviceRequest(
    /// <summary>
    /// Código do dispositivo ESP32 a associar.
    /// Enviar string vazia ("") para desassociar o dispositivo atual.
    /// </summary>
    [Required] string DeviceCode);
}
