using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Enums;
using MeuFinanceiro.Core.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.ViewModels;

public partial class ResumoBancoViewModel : ObservableObject
{
    private readonly ITransacaoRepository _transacaoRepo;
    private readonly IBancoRepository _bancoRepo;

    [ObservableProperty]
    private DateTimeOffset? _dataInicio;

    [ObservableProperty]
    private DateTimeOffset? _dataFim;

    [ObservableProperty]
    private ObservableCollection<ResumoBancoItem> _resumos = new();

    public ResumoBancoViewModel(ITransacaoRepository transacaoRepo, IBancoRepository bancoRepo)
    {
        _transacaoRepo = transacaoRepo;
        _bancoRepo = bancoRepo;

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

        var bancos = await _bancoRepo.GetAllAsync();
        var transacoes = await _transacaoRepo.GetByPeriodoAsync(inicio, fim);

        var itens = new ObservableCollection<ResumoBancoItem>();

        foreach (var banco in bancos)
        {
            var transacoesDoBanco = transacoes
                .Where(t => t.BancoId == banco.Id)
                .ToList();

            var pixDebito = transacoesDoBanco
                .Where(t => t.TipoPagamento == TipoPagamento.PixDebito)
                .OrderBy(t => t.Data)
                .ToList();

            var credito = transacoesDoBanco
                .Where(t => t.TipoPagamento == TipoPagamento.Credito)
                .OrderBy(t => t.Data)
                .ToList();

            itens.Add(new ResumoBancoItem
            {
                Banco = banco,
                PixDebitoTransacoes = new ObservableCollection<Transacao>(pixDebito),
                CreditoTransacoes = new ObservableCollection<Transacao>(credito)
            });
        }

        Resumos = itens;
    }
}

public class ResumoBancoItem
{
    public Banco Banco { get; set; } = null!;
    public ObservableCollection<Transacao> PixDebitoTransacoes { get; set; } = new();
    public ObservableCollection<Transacao> CreditoTransacoes { get; set; } = new();
}