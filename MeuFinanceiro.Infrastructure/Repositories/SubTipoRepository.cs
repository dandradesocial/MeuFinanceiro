using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.Infrastructure.Data;

namespace MeuFinanceiro.Infrastructure.Repositories;

public class SubTipoRepository : BaseRepository<SubTipo>, ISubTipoRepository
{
    public SubTipoRepository(FinanceContext context) : base(context) { }
}