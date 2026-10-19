namespace ArtemisBank.Core.Domain.Interfaces.Repositories
{
    public interface IGenericRepository<Entity> where Entity : class
    {
        Task<Entity?> AddAsync(Entity entity);
        Task<List<Entity>?> AddRangeAsync(List<Entity> entities);
        Task<Entity?> UpdateAsync(int id, Entity entity);
        Task DeleteAsync(int id);
        Task<List<Entity>> GetAllList();
        Task<List<Entity>> GetAllListWithInclude(List<string> properties);
        Task<Entity?> GetById(int id);
        IQueryable<Entity> GetAllQuery();
        IQueryable<Entity> GetAllQueryWithInclude(List<string> properties);
    }
}