using NpgsqlTypes;

namespace kaadebug_bff_api.Models
{
    /// <summary>
    /// Espelha o ENUM health_status do PostgreSQL.
    /// Os valores em snake_case são mapeados pelo Npgsql via NpgsqlEnumAttribute.
    /// </summary>
    public enum HealthStatus
    {
        [PgName("HEALTHY")]
        Healthy,
        [PgName("WARNING")]
        Warning,
        [PgName("CRITICAL")]
        Critical
    }

    /// <summary>
    /// Espelha o ENUM connection_status do PostgreSQL.
    /// </summary>
    public enum ConnectionStatus
    {
        [PgName("ONLINE")]
        Online,
        [PgName("OFFLINE")]
        Offline,
        [PgName("UNASSOCIATED")]
        Unassociated,
        [PgName("NOTFOUND")]
        NotFound

    }

    /// <summary>
    /// Espelha o ENUM sensor_type do PostgreSQL.
    /// </summary>
    public enum SensorType
    {
        [PgName("SOIL_MOISTURE")]
        SoilMoisture,
        [PgName("AIR_HUMIDITY")]
        AirHumidity,
        [PgName("TEMPERATURE")]
        Temperature,
        [PgName("LUMINOSITY")]
        Luminosity
    }

    /// <summary>
    /// Espelha o ENUM notification_priority do PostgreSQL.
    /// </summary>
    public enum NotificationPriority
    {
        [PgName("LOW")]
        Low,
        [PgName("MEDIUM")]
        Medium,
        [PgName("HIGH")]
        High
    }
}
