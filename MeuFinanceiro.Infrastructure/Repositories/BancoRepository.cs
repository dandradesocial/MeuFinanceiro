using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.Infrastructure.Data;

namespace MeuFinanceiro.Infrastructure.Repositories;

public class BancoRepository : BaseRepository<Banco>, IBancoRepository
{
    public BancoRepository(FinanceContext context) : base(context) { }
}