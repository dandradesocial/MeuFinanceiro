using Microsoft.EntityFrameworkCore;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.Infrastructure.Data;

namespace MeuFinanceiro.Infrastructure.Repositories;

public class CategoriaRepository : BaseRepository<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(FinanceContext context) : base(context) { }

    public override async Task<List<Categoria>> GetAllAsync()
    {
        return await _context.Categorias
            .Include(c => c.SubTipo)
            .ToListAsync();
    }
}