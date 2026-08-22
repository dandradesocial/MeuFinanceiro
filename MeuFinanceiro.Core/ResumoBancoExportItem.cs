using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Infrastructure.Export;

public class ResumoBancoExportItem
{
    public Banco Banco { get; set; } = null!;
    public List<Transacao> PixDebitoTransacoes { get; set; } = new();
    public List<Transacao> CreditoTransacoes { get; set; } = new();
}