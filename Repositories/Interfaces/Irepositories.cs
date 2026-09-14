/*
 * Responsabilidade:
 * Define os contratos (interfaces) para a camada de acesso a dados (Repositories).
 * Especifica quais métodos de CRUD e consultas personalizadas cada repositório deve implementar.
 *
 * Papel na arquitetura:
 * Implementa o princípio de Inversão de Dependência (Dependency Inversion) do SOLID.
 * A camada de Services depende destas interfaces, não da implementação concreta que acessa 
 * o Entity Framework. Isso reduz o acoplamento e facilita os testes de unidade.
 * 
 * Observações sobre o design:
 * Possui consultas voltadas para o negócio, como 'GetRecentUnreadByUserAsync' e 
 * 'GetDetailsAsync', indicando que os repositórios não são apenas Wrappers genéricos,
 * mas atendem aos requisitos específicos de montagem de telas (característica forte de um BFF).
 */

using kaadebug_bff_api.Models;

namespace kaadebug_bff_api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }

    public interface ISpeciesRepository
    {
        Task<IEnumerable<Species>> GetAllAsync();
        Task<Species?> GetByIdAsync(Guid id);
    }

    public interface IDeviceRepository
    {
        Task<Device?> GetByIdAsync(Guid id);
        Task<Device?> GetByCodeAsync(string code);
        Task UpdateAsync(Device device);
    }

    public interface IPlantRepository
    {
        Task<IEnumerable<Plant>> GetAllByUserAsync(Guid userId);
        Task<Plant?> GetByIdAsync(Guid id, Guid userId);

        /// <summary>
        /// Retorna a planta com Device, Species e últimas leituras dos sensores (24h)
        /// e notificações recentes — tudo em uma única query para a tela de Detalhes.
        /// </summary>
        Task<Plant?> GetDetailsAsync(Guid id, Guid userId);
        Task AddAsync(Plant plant);
        Task UpdateAsync(Plant plant);
        Task DeleteAsync(Plant plant);
        Task<int> CountByUserAsync(Guid userId);
    }

    public interface ISensorReadingRepository
    {
        Task<IEnumerable<SensorReading>> GetHistoryAsync(
            Guid plantId,
            DateTime from,
            DateTime to);

        /// <summary>
        /// Retorna apenas a leitura mais recente de cada tipo de sensor para uma planta.
        /// Usado para exibir os indicadores atuais na tela de Detalhes.
        /// </summary>
        Task<IEnumerable<SensorReading>> GetLatestByPlantAsync(Guid plantId);
        Task AddRangeAsync(IEnumerable<SensorReading> readings);
    }

    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetAllByUserAsync(Guid userId);
        Task<IEnumerable<Notification>> GetRecentUnreadByUserAsync(Guid userId, int limit = 3);
        Task<Notification?> GetByIdAsync(Guid id, Guid userId);
        Task<int> CountUnreadByUserAsync(Guid userId);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task DeleteAllByUserAsync(Guid userId);
    }

    public interface IDiagnosisRepository
    {
        Task<IEnumerable<DiagnosisResult>> GetByPlantAsync(Guid plantId, Guid userId);
        Task<DiagnosisResult?> GetByIdAsync(Guid id, Guid plantId);
        Task AddAsync(DiagnosisResult diagnosis);
    }
}