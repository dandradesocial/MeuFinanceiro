# MeuFinanceiro

Sistema financeiro pessoal desenvolvido com WinUI 3, .NET, EF Core e SQLite.

## Funcionalidades

- Lançamento de transações (Receitas e Débitos)
- Categorias e Subtipos customizáveis
- Bancos com cores e vínculo de tipo de pagamento
- Resumo por Banco e Resumo Mensal
- Exportação para Excel com ClosedXML
- Backup automático do banco

## Tecnologias

- .NET 10 (preview)
- WinUI 3 (Windows App SDK)
- Entity Framework Core com SQLite
- ClosedXML para Excel
- CommunityToolkit.Mvvm

## Como executar

1. Clone o repositório
2. Abra a solução no Visual Studio 2022+
3. Compile e execute (F5)

## Estrutura

- `MeuFinanceiro.Core`: entidades, enums e interfaces
- `MeuFinanceiro.Infrastructure`: implementação de dados e exportação
- `MeuFinanceiro.UI`: aplicativo WinUI 3

## Contribuição

Sinta-se à vontade para contribuir com melhorias.

## Licença

MIT