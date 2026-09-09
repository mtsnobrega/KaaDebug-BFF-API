/*
 * Responsabilidade:
 * Ponto de entrada da aplicação (Entry Point) e configuração do pipeline HTTP do ASP.NET Core.
 * Este arquivo concentra as configurações de infraestrutura, injeção de dependências (DI), 
 * middleware e integração com serviços externos.
 *
 * Papel na arquitetura:
 * Atua como o "Composition Root". É aqui que as camadas se conectam. Ele registra os
 * Repositories (acesso a dados) e Services (regras de negócio) no contêiner de DI 
 * utilizando o ciclo de vida Scoped, garantindo uma instância por requisição HTTP.
 *
 * Principais configurações:
 * - Banco de Dados: PostgreSQL (Npgsql) com mapeamento nativo de ENUMs em snake_case.
 * - Autenticação: Configuração de JWT Bearer sem tolerância de expiração (ClockSkew = Zero).
 * - Serialização: Configuração do JSON para ignorar nulos e tratar enums como string.
 * - Documentação: Swagger configurado para aceitar tokens JWT.
 * - Segurança: CORS configurado prioritariamente para o aplicativo Mobile (BFF).
 * - Migrations: Aplicação automática de migrations em ambiente de Desenvolvimento.
 */

using kaadebug_bff_api.Infrastructure;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories;
using kaadebug_bff_api.Repositories.Interfaces;
using kaadebug_bff_api.Services;
using kaadebug_bff_api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Npgsql.NameTranslation;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ══════════════════════════════════════════════════════════════════════════════
// 1. BANCO DE DADOS — PostgreSQL com ENUMs nativos
// ══════════════════════════════════════════════════════════════════════════════

// O NpgsqlDataSourceBuilder precisa ser configurado ANTES do AddDbContext
// para que os ENUMs nativos do PostgreSQL sejam reconhecidos corretamente.
// O NpgsqlSnakeCaseNameTranslator converte automaticamente:

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("String de conexão 'DefaultConnection' não encontrada.");

// Uma única instância do tradutor para a aplicação inteira
var snakeCaseTranslator = new NpgsqlSnakeCaseNameTranslator();

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<HealthStatus>("health_status", nameTranslator: snakeCaseTranslator);
dataSourceBuilder.MapEnum<ConnectionStatus>("connection_status", nameTranslator: snakeCaseTranslator);
dataSourceBuilder.MapEnum<SensorType>("sensor_type", nameTranslator: snakeCaseTranslator);
dataSourceBuilder.MapEnum<NotificationPriority>("notification_priority", nameTranslator: snakeCaseTranslator);
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<PlantCareDbContext>(options =>
{
    options.UseNpgsql(dataSource, npgsqlOptions =>
    {
        npgsqlOptions.MapEnum<HealthStatus>("health_status", null, snakeCaseTranslator);
        npgsqlOptions.MapEnum<ConnectionStatus>("connection_status", null, snakeCaseTranslator);
        npgsqlOptions.MapEnum<SensorType>("sensor_type", null, snakeCaseTranslator);
        npgsqlOptions.MapEnum<NotificationPriority>("notification_priority", null, snakeCaseTranslator);
    });

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

// ══════════════════════════════════════════════════════════════════════════════
// 2. AUTENTICAÇÃO JWT
// ══════════════════════════════════════════════════════════════════════════════

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ══════════════════════════════════════════════════════════════════════════════
// 3. REPOSITÓRIOS
// ══════════════════════════════════════════════════════════════════════════════

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISpeciesRepository, SpeciesRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<IPlantRepository, PlantRepository>();
builder.Services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IDiagnosisRepository, DiagnosisRepository>();

// ══════════════════════════════════════════════════════════════════════════════
// 4. SERVIÇOS
// ══════════════════════════════════════════════════════════════════════════════

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ISpeciesService, SpeciesService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IPlantHistoryService, PlantHistoryService>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();
builder.Services.AddScoped<IPlantCareService, PlantCareService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// ══════════════════════════════════════════════════════════════════════════════
// 5. CONTROLLERS + JSON
// ══════════════════════════════════════════════════════════════════════════════

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializa enums como string (ex: "HEALTHY") em vez de int (0) e Remove campos nulos das respostas JSON
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());

        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// ══════════════════════════════════════════════════════════════════════════════
// 6. SWAGGER
// ══════════════════════════════════════════════════════════════════════════════

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PlantCare API",
        Version = "v1",
        Description = "API BFF do aplicativo mobile PlantCare — monitoramento inteligente de plantas via IoT."
    });

    // Configura o Swagger para aceitar JWT no botão Authorize
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT. Exemplo: Bearer eyJhbGci..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ══════════════════════════════════════════════════════════════════════════════
// 7. CORS — permite requisições do app mobile e ferramentas de desenvolvimento
// ══════════════════════════════════════════════════════════════════════════════

builder.Services.AddCors(options =>
{
    options.AddPolicy("MobileApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Em desenvolvimento: aceita qualquer origem (emulador, dispositivo físico, Postman)
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Em produção: restringir para as origens conhecidas
            // TODO: ajustar conforme o domínio de hospedagem
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// ══════════════════════════════════════════════════════════════════════════════
// BUILD
// ══════════════════════════════════════════════════════════════════════════════

var app = builder.Build();

// ══════════════════════════════════════════════════════════════════════════════
// 8. MIDDLEWARE PIPELINE
// A ordem importa: cada middleware só processa a requisição se o anterior
// não a interceptou.
// ══════════════════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PlantCare API v1");
        options.RoutePrefix = string.Empty; // Swagger na raiz: http://localhost:5000
    });
}

app.UseHttpsRedirection();
app.UseCors("MobileApp");

// Autenticação ANTES de autorização — sempre
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ══════════════════════════════════════════════════════════════════════════════
// 9. MIGRAÇÃO AUTOMÁTICA EM DESENVOLVIMENTO
// Garante que o banco está atualizado ao subir a API localmente.
// Em produção: remover este bloco e aplicar migrations manualmente.
// ══════════════════════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<PlantCareDbContext>();

    try
    {
        await db.Database.MigrateAsync();
        app.Logger.LogInformation("Banco de dados atualizado com sucesso.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao aplicar migrations.");
    }
}
app.Run();