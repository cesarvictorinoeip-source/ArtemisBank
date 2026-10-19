using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Interfaces.Repositories;

namespace ArtemisBank.Core.Application.Services
{
    public class GenericService<T, TDto> : IGenericService<T, TDto>
        where T : class
        where TDto : class
    {
        private readonly IGenericRepository<T> _repository;
        private readonly IMapper _mapper;

        public GenericService(IGenericRepository<T> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public virtual async Task<TDto> AddAsync(TDto dto)
        {
            var entity = _mapper.Map<T>(dto);
            var result = await _repository.AddAsync(entity);
            return _mapper.Map<TDto>(result!);
        }

        public virtual async Task<List<TDto>> AddRangeAsync(List<TDto> dtos)
        {
            var entities = _mapper.Map<List<T>>(dtos);
            var result = await _repository.AddRangeAsync(entities);
            return _mapper.Map<List<TDto>>(result!);
        }

        public virtual async Task<TDto?> UpdateAsync(int id, TDto dto)
        {
            var entity = _mapper.Map<T>(dto);
            var result = await _repository.UpdateAsync(id, entity);
            return _mapper.Map<TDto>(result);
        }

        public virtual async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public virtual async Task<List<TDto>> GetAllList()
        {
            var entities = await _repository.GetAllList();
            return _mapper.Map<List<TDto>>(entities);
        }

        public virtual async Task<TDto?> GetById(int id)
        {
            var entity = await _repository.GetById(id);
            if (entity == null) return null;
            return _mapper.Map<TDto>(entity);
        }
    }
}