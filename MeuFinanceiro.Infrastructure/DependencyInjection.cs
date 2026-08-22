using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.Infrastructure.Data;
using MeuFinanceiro.Infrastructure.Export;
using MeuFinanceiro.Infrastructure.Repositories;

namespace MeuFinanceiro.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FinanceContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IBancoRepository, BancoRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<ITransacaoRepository, TransacaoRepository>();
        services.AddScoped<ISubTipoRepository, SubTipoRepository>(); // novo
        services.AddScoped<IExportadorExcel, ExportadorExcel>();

        return services;
    }
}