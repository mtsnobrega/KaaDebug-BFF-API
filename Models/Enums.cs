namespace kaadebug_bff_api.Models
{
    /// <summary>
    /// Espelha o ENUM health_status do PostgreSQL.
    /// Os valores em snake_case são mapeados pelo Npgsql via NpgsqlEnumAttribute.
    /// </summary>
    public enum HealthStatus
    {
        Healthy,
        Warning,
        Critical
    }

    /// <summary>
    /// Espelha o ENUM connection_status do PostgreSQL.
    /// </summary>
    public enum ConnectionStatus
    {
        Online,
        Offline,
        Unassociated
    }

    /// <summary>
    /// Espelha o ENUM sensor_type do PostgreSQL.
    /// </summary>
    public enum SensorType
    {
        SoilMoisture,
        AirHumidity,
        Temperature,
        Luminosity
    }

    /// <summary>
    /// Espelha o ENUM notification_priority do PostgreSQL.
    /// </summary>
    public enum NotificationPriority
    {
        Low,
        Medium,
        High
    }
}
