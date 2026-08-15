using kaadebug_bff_api.Models;
using Microsoft.EntityFrameworkCore;


namespace kaadebug_bff_api.Infrastructure
{
    public class PlantCareDbContext : DbContext
    {
        public PlantCareDbContext(DbContextOptions<PlantCareDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Species> Species => Set<Species>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<Plant> Plants => Set<Plant>();
        public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<DiagnosisResult> DiagnosisResults => Set<DiagnosisResult>();

        // ── ENUMs nativos do PostgreSQL ───────────────────────────────────────
        // O Npgsql mapeia os ENUMs C# para os ENUMs do PostgreSQL usando
        // snake_case por padrão. Os valores abaixo correspondem exatamente
        // aos definidos nos scripts de criação do banco.
        /*
        modelBuilder.HasPostgresEnum<HealthStatus>(
            "health_status",
            new[] { "HEALTHY", "WARNING", "CRITICAL" });

        modelBuilder.HasPostgresEnum<ConnectionStatus>(
            "connection_status",
            new[] { "ONLINE", "OFFLINE", "UNASSOCIATED" });

        modelBuilder.HasPostgresEnum<SensorType>(
            "sensor_type",
            new[] { "SOIL_MOISTURE", "AIR_HUMIDITY", "TEMPERATURE", "LUMINOSITY" });

        modelBuilder.HasPostgresEnum<NotificationPriority>(
            "notification_priority",
            new[] { "LOW", "MEDIUM", "HIGH" });
        */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapeie TODAS as enums e passe o nome exato do tipo no PostgreSQL
            modelBuilder.HasPostgresEnum<HealthStatus>("health_status");
            modelBuilder.HasPostgresEnum<ConnectionStatus>("connection_status");
            modelBuilder.HasPostgresEnum<SensorType>("sensor_type");
            modelBuilder.HasPostgresEnum<NotificationPriority>("notification_priority");

            // ── Users ─────────────────────────────────────────────────────────────
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(u => u.Name)
                      .HasColumnName("name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(u => u.Email)
                      .HasColumnName("email")
                      .HasMaxLength(250)
                      .IsRequired();

                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.PasswordHash)
                      .HasColumnName("password_hash")
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(u => u.NotificationsEnabled)
                      .HasColumnName("notifications_enabled")
                      .HasDefaultValue(true);

                entity.Property(u => u.CriticalAlertsOnly)
                      .HasColumnName("critical_alerts_only")
                      .HasDefaultValue(false);

                entity.Property(u => u.CreatedAt)
                      .HasColumnName("created_at")
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();
            });

            // ── Species ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Species>(entity =>
            {
                entity.ToTable("species");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(s => s.Name)
                      .HasColumnName("name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.HasIndex(s => s.Name).IsUnique();

                entity.Property(s => s.PhotoUrl)
                      .HasColumnName("photo_url")
                      .HasMaxLength(500);

                entity.Property(s => s.SoilMoistureMin).HasColumnName("soil_moisture_min").HasPrecision(5, 2);
                entity.Property(s => s.SoilMoistureMax).HasColumnName("soil_moisture_max").HasPrecision(5, 2);
                entity.Property(s => s.AirHumidityMin).HasColumnName("air_humidity_min").HasPrecision(5, 2);
                entity.Property(s => s.AirHumidityMax).HasColumnName("air_humidity_max").HasPrecision(5, 2);
                entity.Property(s => s.TemperatureMin).HasColumnName("temperature_min").HasPrecision(5, 2);
                entity.Property(s => s.TemperatureMax).HasColumnName("temperature_max").HasPrecision(5, 2);
                entity.Property(s => s.LuminosityMin).HasColumnName("luminosity_min").HasPrecision(10, 2);
                entity.Property(s => s.LuminosityMax).HasColumnName("luminosity_max").HasPrecision(10, 2);

                // JSONB — o Npgsql serializa/desserializa JsonDocument automaticamente
                entity.Property(s => s.CareInfo)
                      .HasColumnName("care_info")
                      .HasColumnType("jsonb");
            });

            // ── Devices ───────────────────────────────────────────────────────────
            modelBuilder.Entity<Device>(entity =>
            {
                entity.ToTable("devices");
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(d => d.Code)
                      .HasColumnName("code")
                      .HasMaxLength(50)
                      .IsRequired();

                entity.HasIndex(d => d.Code).IsUnique();

                entity.Property(d => d.ConnectionStatus)
                      .HasColumnName("connection_status")
                      .HasColumnType("connection_status")
                      .HasDefaultValue(ConnectionStatus.Unassociated);

                entity.Property(d => d.LastHeartbeatAt)
                      .HasColumnType("timestamp without time zone")
                      .HasColumnName("last_heartbeat_at");

                entity.Property(d => d.RegisteredAt)
                      .HasColumnName("registered_at")
                      .HasColumnType("timestamp without time zone")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ── Plants ────────────────────────────────────────────────────────────
            modelBuilder.Entity<Plant>(entity =>
            {
                entity.ToTable("plants");
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(p => p.UserId).HasColumnName("user_id");
                entity.Property(p => p.SpeciesId).HasColumnName("species_id");
                entity.Property(p => p.DeviceId).HasColumnName("device_id");

                entity.Property(p => p.Name)
                      .HasColumnName("name")
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(p => p.PhotoUrl)
                      .HasColumnName("photo_url")
                      .HasMaxLength(500);

                entity.Property(p => p.HealthStatus)
                      .HasColumnName("health_status")
                      .HasColumnType("health_status")
                      .HasDefaultValue(HealthStatus.Healthy);

                entity.Property(p => p.StatusReason)
                      .HasColumnName("status_reason")
                      .HasMaxLength(300);

                entity.Property(p => p.CreatedAt)
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();

                entity.Property(p => p.UpdatedAt)
                      .HasColumnName("updated_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relacionamentos
                entity.HasOne(p => p.User)
                      .WithMany(u => u.Plants)
                      .HasForeignKey(p => p.UserId)
                      .HasConstraintName("fk_plants_user")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Species)
                      .WithMany(s => s.Plants)
                      .HasForeignKey(p => p.SpeciesId)
                      .HasConstraintName("fk_plants_species")
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Device)
                      .WithOne(d => d.Plant)
                      .HasForeignKey<Plant>(p => p.DeviceId)
                      .HasConstraintName("fk_plants_device")
                      .OnDelete(DeleteBehavior.SetNull)
                      .IsRequired(false);

                entity.HasMany(p => p.SensorReadings)
                      .WithOne(s => s.Plant)
                      .HasForeignKey(s => s.PlantId)
                      .HasConstraintName("fk_sensor_readings_plant")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Notifications)
                      .WithOne(n => n.Plant)
                      .HasForeignKey(n => n.PlantId)
                      .HasConstraintName("fk_notifications_plant")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── SensorReadings ────────────────────────────────────────────────────
            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.ToTable("sensor_readings");

                // BIGINT GENERATED ALWAYS AS IDENTITY
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Id)
                      .HasColumnName("id")
                      .UseIdentityAlwaysColumn(); // equivalente ao GENERATED ALWAYS AS IDENTITY

                entity.Property(r => r.PlantId).HasColumnName("plant_id");
                entity.Property(r => r.DeviceId).HasColumnName("device_id");

                entity.Property(r => r.SensorType)
                      .HasColumnName("sensor_type")
                      .HasColumnType("sensor_type");

                entity.Property(r => r.Value)
                      .HasColumnName("value")
                      .HasPrecision(10, 2);

                entity.Property(r => r.IsWithinIdealRange)
                      .HasColumnName("is_within_ideal_range");

                entity.Property(r => r.ReadAt)
                      .HasColumnName("read_at");

                // Índice em (plant_id, read_at) — chave para as queries de histórico
                entity.HasIndex(r => new { r.PlantId, r.ReadAt })
                      .HasDatabaseName("ix_sensor_readings_plant_id_read_at");

                entity.HasOne(r => r.Plant)
                      .WithMany(p => p.SensorReadings)
                      .HasForeignKey(r => r.PlantId)
                      .HasConstraintName("fk_sensor_readings_plant")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Device)
                      .WithMany(d => d.SensorReadings)
                      .HasForeignKey(r => r.DeviceId)
                      .HasConstraintName("fk_sensor_readings_device")
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ── Notifications ─────────────────────────────────────────────────────
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(n => n.UserId).HasColumnName("user_id");
                entity.Property(n => n.PlantId).HasColumnName("plant_id");

                entity.Property(n => n.Message)
                      .HasColumnName("message")
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(n => n.Priority)
                      .HasColumnName("priority")
                      .HasColumnType("notification_priority")
                      .HasDefaultValue(NotificationPriority.Low);

                entity.Property(n => n.IsRead)
                      .HasColumnName("is_read")
                      .HasDefaultValue(false);

                entity.Property(n => n.CreatedAt)
                      .HasColumnName("created_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP")
                      .ValueGeneratedOnAdd();

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .HasConstraintName("fk_notifications_user")
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Plant)
                      .WithMany(p => p.Notifications)
                      .HasForeignKey(n => n.PlantId)
                      .HasConstraintName("fk_notifications_plant")
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── DiagnosisResults ──────────────────────────────────────────────────
            modelBuilder.Entity<DiagnosisResult>(entity =>
            {
                entity.ToTable("diagnosis_results");
                entity.HasKey(d => d.Id);

                entity.Property(d => d.Id)
                      .HasColumnName("id")
                      .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(d => d.PlantId).HasColumnName("plant_id");

                entity.Property(d => d.ImageUrl)
                      .HasColumnName("image_url")
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(d => d.IsHealthy)
                      .HasColumnName("is_healthy");

                entity.Property(d => d.OverallObservation)
                      .HasColumnName("overall_observation")
                      .HasMaxLength(1000);

                entity.Property(d => d.IssuesJson)
                      .HasColumnName("issues_json")
                      .HasColumnType("jsonb");

                entity.Property(d => d.PerformedAt)
                      .HasColumnName("performed_at")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Plant)
                      .WithMany(p => p.DiagnosisResults)
                      .HasForeignKey(d => d.PlantId)
                      .HasConstraintName("fk_diagnosis_results_plant")
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
