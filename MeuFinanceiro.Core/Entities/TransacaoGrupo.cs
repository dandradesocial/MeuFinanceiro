using MeuFinanceiro.Core.Entities;

namespace MeuFinanceiro.Infrastructure.Export;

public class TransacaoGrupo
{
    public string Nome { get; set; } = string.Empty;
    public IEnumerable<Transacao> Transacoes { get; set; } = Enumerable.Empty<Transacao>();
}