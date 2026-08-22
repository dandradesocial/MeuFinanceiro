using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Core.Interfaces;

public interface ISubTipoRepository
{
    Task<List<SubTipo>> GetAllAsync();
    Task<SubTipo?> GetByIdAsync(Guid id);
    Task AddAsync(SubTipo subtipo);
    Task UpdateAsync(SubTipo subtipo);
    Task DeleteAsync(SubTipo subtipo);
}