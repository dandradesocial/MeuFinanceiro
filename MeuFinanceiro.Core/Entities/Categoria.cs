using MeuFinanceiro.Core.Enums;

namespace MeuFinanceiro.Core.Entities;

public class Categoria
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public TipoTransacao Tipo { get; set; } // Mantido para simplificar filtros e exibição
    public Guid SubTipoId { get; set; }     // Novo relacionamento
    public SubTipo? SubTipo { get; set; }
    public string? CorHex { get; set; }
}