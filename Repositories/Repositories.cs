using kaadebug_bff_api.Infrastructure;
using kaadebug_bff_api.Models;
using kaadebug_bff_api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace kaadebug_bff_api.Repositories
{
    /*
     * Responsabilidade:
     * Gerencia as operações de banco de dados (CRUD e consultas específicas) para a entidade User.
     *
     * Papel na arquitetura:
     * Isola a lógica de persistência e conversão de DateTime (UTC) da entidade User.
     */
    public class UserRepository : IUserRepository
    {
        private readonly PlantCareDbContext _context;
        public UserRepository(PlantCareDbContext context) => _context = context;

        public Task<User?> GetByIdAsync(Guid id) =>
            _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        public Task<User?> GetByEmailAsync(string email) =>
            _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

        public Task<bool> EmailExistsAsync(string email) =>
            _context.Users.AnyAsync(u => u.Email == email.ToLower());

        public async Task AddAsync(User user)
        {
            user.Email = user.Email.ToLower();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            if (user.CreatedAt.Kind == DateTimeKind.Unspecified)
            {
                user.CreatedAt = DateTime.SpecifyKind(
                    user.CreatedAt,
                    DateTimeKind.Utc
                );
            }
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }

    /*
     * Responsabilidade:
     * Gerencia a leitura do catálogo mestre de Espécies (Species).
     *
     * Observação:
     * Como se trata de um catálogo pré-populado, possui apenas métodos de leitura.
     */
    public class SpeciesRepository : ISpeciesRepository
    {
        private readonly PlantCareDbContext _context;
        public SpeciesRepository(PlantCareDbContext context) => _context = context;

        public Task<IEnumerable<Species>> GetAllAsync() =>
            Task.FromResult<IEnumerable<Species>>(
                _context.Species.OrderBy(s => s.Name).AsEnumerable());

        public Task<Species?> GetByIdAsync(Guid id) =>
            _context.Species.FirstOrDefaultAsync(s => s.Id == id);
    }

    /*
     * Responsabilidade:
     * Gerencia a busca e atualização de dispositivos de hardware (Devices / ESP32).
     */
    public class DeviceRepository : IDeviceRepository
    {
        private readonly PlantCareDbContext _context;
        public DeviceRepository(PlantCareDbContext context) => _context = context;

        public Task<Device?> GetByIdAsync(Guid id) =>
            _context.Devices.FirstOrDefaultAsync(d => d.Id == id);

        public Task<Device?> GetByCodeAsync(string code) =>
            _context.Devices
            .Include(d => d.Plant)
            .FirstOrDefaultAsync(d => d.Code == code.ToUpper());


        public async Task UpdateAsync(Device device)
        {
            _context.Devices.Update(device);
            await _context.SaveChangesAsync();
        }
    }

    /*
     * Responsabilidade:
     * Repositório central da aplicação, gerenciando as Plantas cadastradas pelos usuários.
     *
     * Padrão Arquitetural Observado (Aggregate Fetching):
     * O método 'GetDetailsAsync' demonstra o uso da API como BFF. Ele realiza Eager Loading 
     * (.Include) de múltiplos relacionamentos filtrados (Notificações não lidas, sensores das 
     * últimas 24h) para entregar um objeto completo pronto para a tela de Detalhes do Mobile.
     */
    public class PlantRepository : IPlantRepository
    {
        private readonly PlantCareDbContext _context;
        public PlantRepository(PlantCareDbContext context) => _context = context;

        public async Task<IEnumerable<Plant>> GetAllByUserAsync(Guid userId) =>
            await _context.Plants
                .Include(p => p.Species)
                .Include(p => p.Device)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt)
                .ToListAsync();

        public Task<Plant?> GetByIdAsync(Guid id, Guid userId) =>
            _context.Plants
                .Include(p => p.Species)
                .Include(p => p.Device)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

        public async Task<Plant?> GetDetailsAsync(Guid id, Guid userId)
        {
            var cutoff24h = DateTime.UtcNow.AddHours(-24);

            return await _context.Plants
                .Include(p => p.Species)
                .Include(p => p.Device)
                .Include(p => p.Notifications
                    .Where(n => !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(5))
                .Include(p => p.SensorReadings
                    .Where(r => r.ReadAt >= cutoff24h)
                    .OrderBy(r => r.ReadAt))
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task AddAsync(Plant plant)
        {
            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Plant plant)
        {
            // Garante que o Entity Framework entenda o CreatedAt como UTC
            if (plant.CreatedAt.Kind == DateTimeKind.Unspecified)
            {
                plant.CreatedAt = DateTime.SpecifyKind(plant.CreatedAt, DateTimeKind.Utc);
            }

            plant.UpdatedAt = DateTime.UtcNow;
            _context.Plants.Update(plant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Plant plant)
        {
            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
        }
        public Task<int> CountByUserAsync(Guid userId) =>
            _context.Plants.CountAsync(p => p.UserId == userId);
    }

    /*
     * Responsabilidade:
     * Acesso aos dados históricos gerados pelos dispositivos IoT.
     *
     * Otimização:
     * O método 'GetLatestByPlantAsync' itera sobre os tipos de sensores conhecidos 
     * e busca apenas o último registro de cada, evitando trazer milhares de linhas 
     * desnecessárias para a memória.
     */
    public class SensorReadingRepository : ISensorReadingRepository
    {
        private readonly PlantCareDbContext _context;
        public SensorReadingRepository(PlantCareDbContext context) => _context = context;

        public async Task<IEnumerable<SensorReading>> GetHistoryAsync(
            Guid plantId, DateTime from, DateTime to) =>
            await _context.SensorReadings
                .Where(r => r.PlantId == plantId && r.ReadAt >= from && r.ReadAt <= to)
                .OrderBy(r => r.ReadAt)
                .ToListAsync();

        // Retorna a leitura mais recente de cada tipo de sensor
        public async Task<IEnumerable<SensorReading>> GetLatestByPlantAsync(Guid plantId)
        {
            
            var sensorTypes = Enum.GetValues<SensorType>();
            var results = new List<SensorReading>();

            foreach (var type in sensorTypes)
            {
                var latest = await _context.SensorReadings
                    .Where(r => r.PlantId == plantId && r.SensorType == type)
                    .OrderByDescending(r => r.ReadAt)
                    .FirstOrDefaultAsync();

                if (latest is not null)
                    results.Add(latest);
            }
            return results;
        }
        public async Task AddRangeAsync(IEnumerable<SensorReading> readings)
        {
            _context.SensorReadings.AddRange(readings);
            await _context.SaveChangesAsync();
        }
    }

    /*
     * Responsabilidade:
     * Gerenciar alertas e mensagens enviadas aos usuários.
     * Contém validações implícitas de posse (exige o userId nas consultas).
     */
    public class NotificationRepository : INotificationRepository
    {
        private readonly PlantCareDbContext _context;
        public NotificationRepository(PlantCareDbContext context) => _context = context;

        public async Task<IEnumerable<Notification>> GetAllByUserAsync(Guid userId) =>
            await _context.Notifications
                .Include(n => n.Plant)
                .Where(n => n.UserId == userId)
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Notification>> GetRecentUnreadByUserAsync(Guid userId, int limit = 3) =>
            await _context.Notifications
                .Include(n => n.Plant)
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(limit)
                .ToListAsync();

        public Task<Notification?> GetByIdAsync(Guid id, Guid userId) =>
            _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        public Task<int> CountUnreadByUserAsync(Guid userId) =>
            _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAllByUserAsync(Guid userId)
        {
            var notifications = _context.Notifications.Where(n => n.UserId == userId);
            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }
    }

    /*
     * Responsabilidade:
     * Gerenciar os laudos gerados (possivelmente por IA externa) sobre a saúde da planta.
     * Garante que um usuário só pode acessar diagnósticos de plantas que lhe pertencem
     * realizando Join implícito com a entidade Plant via LINQ (.Where(d => d.Plant.UserId == userId)).
     */
    public class DiagnosisRepository : IDiagnosisRepository
    {
        private readonly PlantCareDbContext _context;
        public DiagnosisRepository(PlantCareDbContext context) => _context = context;

        public async Task<IEnumerable<DiagnosisResult>> GetByPlantAsync(Guid plantId, Guid userId) =>
            await _context.DiagnosisResults
                .Include(d => d.Plant)
                .Where(d => d.PlantId == plantId && d.Plant.UserId == userId)
                .OrderByDescending(d => d.PerformedAt)
                .ToListAsync();

        public Task<DiagnosisResult?> GetByIdAsync(Guid id, Guid plantId) =>
            _context.DiagnosisResults
                .FirstOrDefaultAsync(d => d.Id == id && d.PlantId == plantId);

        public async Task AddAsync(DiagnosisResult diagnosis)
        {
            _context.DiagnosisResults.Add(diagnosis);
            await _context.SaveChangesAsync();
        }
    }
}