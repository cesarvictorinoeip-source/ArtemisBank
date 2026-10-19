namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IGenericService<T, TDto>
        where T : class
        where TDto : class
    {
        Task<TDto> AddAsync(TDto dto);
        Task<List<TDto>> AddRangeAsync(List<TDto> dtos);
        Task<TDto?> UpdateAsync(int id, TDto dto);
        Task DeleteAsync(int id);
        Task<List<TDto>> GetAllList();
        Task<TDto?> GetById(int id);
    }
}