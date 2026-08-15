using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using kaadebug_bff_api.Models;

#nullable disable

namespace kaadebug_bff_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlantIdToDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:connection_status", "NOTFOUND,OFFLINE,ONLINE,UNASSOCIATED")
                .Annotation("Npgsql:Enum:connection_status.connection_status", "ONLINE,OFFLINE,UNASSOCIATED,NOTFOUND")
                .Annotation("Npgsql:Enum:health_status", "CRITICAL,HEALTHY,WARNING")
                .Annotation("Npgsql:Enum:health_status.health_status", "HEALTHY,WARNING,CRITICAL")
                .Annotation("Npgsql:Enum:notification_priority", "HIGH,LOW,MEDIUM")
                .Annotation("Npgsql:Enum:notification_priority.notification_priority", "LOW,MEDIUM,HIGH")
                .Annotation("Npgsql:Enum:sensor_type", "AIR_HUMIDITY,LUMINOSITY,SOIL_MOISTURE,TEMPERATURE")
                .Annotation("Npgsql:Enum:sensor_type.sensor_type", "SOIL_MOISTURE,AIR_HUMIDITY,TEMPERATURE,LUMINOSITY");

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    connection_status = table.Column<ConnectionStatus>(type: "connection_status", nullable: false, defaultValue: ConnectionStatus.Unassociated),
                    last_heartbeat_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    registered_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PlantId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "species",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    soil_moisture_min = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    soil_moisture_max = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    air_humidity_min = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    air_humidity_max = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    temperature_min = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    temperature_max = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    luminosity_min = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    luminosity_max = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    care_info = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_species", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    notifications_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    critical_alerts_only = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "plants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    species_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    photo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    health_status = table.Column<HealthStatus>(type: "health_status", nullable: false, defaultValue: HealthStatus.Healthy),
                    status_reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plants", x => x.id);
                    table.ForeignKey(
                        name: "fk_plants_device",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_plants_species",
                        column: x => x.species_id,
                        principalTable: "species",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_plants_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "diagnosis_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    plant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_healthy = table.Column<bool>(type: "boolean", nullable: false),
                    overall_observation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    issues_json = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_diagnosis_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_diagnosis_results_plant",
                        column: x => x.plant_id,
                        principalTable: "plants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    priority = table.Column<NotificationPriority>(type: "notification_priority", nullable: false, defaultValue: NotificationPriority.Low),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_plant",
                        column: x => x.plant_id,
                        principalTable: "plants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensor_readings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    plant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_type = table.Column<SensorType>(type: "sensor_type", nullable: false),
                    value = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    is_within_ideal_range = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sensor_readings", x => x.id);
                    table.ForeignKey(
                        name: "fk_sensor_readings_device",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_sensor_readings_plant",
                        column: x => x.plant_id,
                        principalTable: "plants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_devices_code",
                table: "devices",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_diagnosis_results_plant_id",
                table: "diagnosis_results",
                column: "plant_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_plant_id",
                table: "notifications",
                column: "plant_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_plants_device_id",
                table: "plants",
                column: "device_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plants_species_id",
                table: "plants",
                column: "species_id");

            migrationBuilder.CreateIndex(
                name: "IX_plants_user_id",
                table: "plants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sensor_readings_device_id",
                table: "sensor_readings",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "ix_sensor_readings_plant_id_read_at",
                table: "sensor_readings",
                columns: new[] { "plant_id", "read_at" });

            migrationBuilder.CreateIndex(
                name: "IX_species_name",
                table: "species",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "diagnosis_results");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "sensor_readings");

            migrationBuilder.DropTable(
                name: "plants");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "species");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
