using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.ViewModels;

public partial class ResumoMensalViewModel : ObservableObject
{
    private readonly ITransacaoRepository _transacaoRepo;
    private readonly ICategoriaRepository _categoriaRepo;
    private readonly ISubTipoRepository _subTipoRepo;

    [ObservableProperty]
    private DateTimeOffset? _dataInicio;

    [ObservableProperty]
    private DateTimeOffset? _dataFim;

    [ObservableProperty]
    private ObservableCollection<ResumoMensalItem> _resumos = new();

    public ResumoMensalViewModel(
        ITransacaoRepository transacaoRepo,
        ICategoriaRepository categoriaRepo,
        ISubTipoRepository subTipoRepo)
    {
        _transacaoRepo = transacaoRepo;
        _categoriaRepo = categoriaRepo;
        _subTipoRepo = subTipoRepo;

        var hoje = DateTime.Today;
        DataInicio = new DateTimeOffset(hoje.Year, hoje.Month, 1, 0, 0, 0, TimeSpan.Zero);
        DataFim = DataInicio.Value.AddMonths(1).AddDays(-1);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (!DataInicio.HasValue || !DataFim.HasValue)
            return;

        var inicio = DataInicio.Value.Date;
        var fim = DataFim.Value.Date.AddDays(1).AddTicks(-1);

        var subtipos = await _subTipoRepo.GetAllAsync();
        var categorias = await _categoriaRepo.GetAllAsync();
        var transacoes = await _transacaoRepo.GetByPeriodoAsync(inicio, fim);

        var itens = new ObservableCollection<ResumoMensalItem>();

        // Mantém a ordem dos subtipos igual à aba Lançamentos
        foreach (var subtipo in subtipos.OrderBy(s => s.Tipo).ThenBy(s => s.Nome))
        {
            var categoriasDoSubtipo = categorias
                .Where(c => c.SubTipoId == subtipo.Id)
                .OrderBy(c => c.Nome)
                .ToList();

            var categoriasTotais = new ObservableCollection<CategoriaTotal>();

            foreach (var categoria in categoriasDoSubtipo)
            {
                var transacoesDaCategoria = transacoes
                    .Where(t => t.CategoriaId == categoria.Id)
                    .ToList();

                if (transacoesDaCategoria.Count == 0)
                    continue;

                // Soma com sinal: Receita soma, Débito subtrai
                decimal total = transacoesDaCategoria.Sum(t =>
                    t.Tipo == MeuFinanceiro.Core.Enums.TipoTransacao.Receita ? t.Valor : -t.Valor);

                categoriasTotais.Add(new CategoriaTotal
                {
                    CategoriaNome = categoria.Nome,
                    Total = total
                });
            }

            if (categoriasTotais.Count == 0)
                continue;

            itens.Add(new ResumoMensalItem
            {
                SubtipoNome = subtipo.Nome,
                CategoriasTotais = categoriasTotais
            });
        }

        Resumos = itens;
    }
}

public class ResumoMensalItem
{
    public string SubtipoNome { get; set; } = string.Empty;
    public ObservableCollection<CategoriaTotal> CategoriasTotais { get; set; } = new();
}

public class CategoriaTotal
{
    public string CategoriaNome { get; set; } = string.Empty;
    public decimal Total { get; set; }
}