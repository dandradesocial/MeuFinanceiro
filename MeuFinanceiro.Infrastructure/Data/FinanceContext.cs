using Microsoft.EntityFrameworkCore;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Enums;

namespace MeuFinanceiro.Infrastructure.Data;

public class FinanceContext : DbContext
{
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transacao> Transacoes => Set<Transacao>();
    public DbSet<SubTipo> SubTipos => Set<SubTipo>();

    public FinanceContext(DbContextOptions<FinanceContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações de entidade (mantidas)
        modelBuilder.Entity<Banco>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Nome).IsRequired().HasMaxLength(100);
            b.Property(x => x.CorHex).HasMaxLength(9);
        });

        modelBuilder.Entity<SubTipo>(st =>
        {
            st.HasKey(x => x.Id);
            st.Property(x => x.Nome).IsRequired().HasMaxLength(100);
            st.Property(x => x.Tipo).HasConversion<int>();
            st.Property(x => x.VincularBanco).IsRequired();
        });

        modelBuilder.Entity<Categoria>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.Nome).IsRequired().HasMaxLength(100);
            c.Property(x => x.Tipo).HasConversion<int>();
            c.Property(x => x.CorHex).HasMaxLength(9);

            c.HasOne(x => x.SubTipo)
                .WithMany()
                .HasForeignKey(x => x.SubTipoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transacao>(t =>
        {
            t.HasKey(x => x.Id);
            t.Property(x => x.Valor).HasColumnType("decimal(18,2)");
            t.Property(x => x.Tipo).HasConversion<int>();
            t.Property(x => x.TipoPagamento).HasConversion<int?>();
            t.Property(x => x.Observacao).HasMaxLength(500);
            t.Ignore(x => x.SubtipoDebito);

            t.HasOne(x => x.Banco)
                .WithMany()
                .HasForeignKey(x => x.BancoId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            t.HasOne(x => x.Categoria)
                .WithMany()
                .HasForeignKey(x => x.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed de subtipos padrão
        var receitaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var reservaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var investimentoId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var gastoFixoId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var gastoVariavelId = Guid.Parse("55555555-5555-5555-5555-555555555555");

        modelBuilder.Entity<SubTipo>().HasData(
            new SubTipo { Id = receitaId, Nome = "Receita", Tipo = TipoTransacao.Receita, VincularBanco = false },
            new SubTipo { Id = reservaId, Nome = "Reserva", Tipo = TipoTransacao.Debito, VincularBanco = true },
            new SubTipo { Id = investimentoId, Nome = "Investimento", Tipo = TipoTransacao.Debito, VincularBanco = true },
            new SubTipo { Id = gastoFixoId, Nome = "Gasto Fixo", Tipo = TipoTransacao.Debito, VincularBanco = true },
            new SubTipo { Id = gastoVariavelId, Nome = "Gasto Variável", Tipo = TipoTransacao.Debito, VincularBanco = true }
        );
    }
}