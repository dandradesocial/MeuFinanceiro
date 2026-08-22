using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Infrastructure.Export;

public interface IExportadorExcel
{
    Task<byte[]> ExportarTransacoesAsync(IEnumerable<Transacao> transacoes);
    Task<byte[]> ExportarTransacoesAgrupadasAsync(IEnumerable<TransacaoGrupo> grupos);
    Task<byte[]> ExportarPlanilhaCompletaAsync(
        IEnumerable<TransacaoGrupo> gruposLancamentos,
        IEnumerable<ResumoBancoExportItem> resumoBancos);
}