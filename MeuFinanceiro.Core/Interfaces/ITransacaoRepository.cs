using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Core.Interfaces;

public interface ITransacaoRepository
{
    Task<List<Transacao>> GetAllAsync();
    Task<Transacao?> GetByIdAsync(Guid id);
    Task AddAsync(Transacao transacao);
    Task UpdateAsync(Transacao transacao);
    Task DeleteAsync(Transacao transacao);
    Task<List<Transacao>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
}