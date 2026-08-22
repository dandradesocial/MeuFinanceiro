namespace MeuFinanceiro.Core.Entities;

public class Banco
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string CorHex { get; set; } = "#000000";
}