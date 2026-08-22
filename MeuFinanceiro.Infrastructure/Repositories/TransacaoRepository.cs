using Microsoft.EntityFrameworkCore;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.Infrastructure.Data;

namespace MeuFinanceiro.Infrastructure.Repositories;

public class TransacaoRepository : BaseRepository<Transacao>, ITransacaoRepository
{
    public TransacaoRepository(FinanceContext context) : base(context) { }

    public async Task<List<Transacao>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _dbSet
            .Include(t => t.Banco)
            .Include(t => t.Categoria)
                .ThenInclude(c => c.SubTipo)
            .Where(t => t.Data >= inicio && t.Data <= fim)
            .OrderBy(t => t.Data)
            .ToListAsync();
    }
}