using MeuFinanceiro.Core.Entities;
using System.Collections.ObjectModel;

namespace MeuFinanceiro.UI.Models;

public class AbaLancamento
{
    public string Nome { get; set; } = string.Empty;
    public ObservableCollection<Transacao> Transacoes { get; set; } = new();
}