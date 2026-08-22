using System.ComponentModel.DataAnnotations.Schema;
using MeuFinanceiro.Core.Enums;

namespace MeuFinanceiro.Core.Entities;

public class Transacao
{
    public Guid Id { get; set; }
    public DateTime Data { get; set; }
    public TipoTransacao Tipo { get; set; }          // Receita ou Débito
    public decimal Valor { get; set; }
    public string? Observacao { get; set; }

    public Guid? BancoId { get; set; }
    public Banco? Banco { get; set; }

    public Guid CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public TipoPagamento? TipoPagamento { get; set; } // Novo campo (null se não houver banco)

    [NotMapped]
    public string? SubtipoDebito => Categoria?.SubTipo?.Nome;
}