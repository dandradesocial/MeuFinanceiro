using MeuFinanceiro.Core.Enums;

namespace MeuFinanceiro.Core.Entities;

public class SubTipo
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoTransacao Tipo { get; set; }
    public bool VincularBanco { get; set; } // Novo campo
}