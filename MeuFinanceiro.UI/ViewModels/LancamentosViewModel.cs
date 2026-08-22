using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Enums;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.Infrastructure.Export;
using MeuFinanceiro.UI.Models;
using MeuFinanceiro.UI.Services;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.ViewModels;

public partial class LancamentosViewModel : ObservableObject
{
    private readonly ITransacaoRepository _transacaoRepo;
    private readonly IBancoRepository _bancoRepo;
    private readonly ICategoriaRepository _categoriaRepo;
    private readonly ISubTipoRepository _subTipoRepo;
    private readonly IExportadorExcel _exportador;
    private readonly IFileSaveService _fileSaveService;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private DateTimeOffset? _dataInicio;

    [ObservableProperty]
    private DateTimeOffset? _dataFim;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<Transacao> _transacoes = new();

    [ObservableProperty]
    private ObservableCollection<Banco> _bancos = new();

    [ObservableProperty]
    private ObservableCollection<Categoria> _categorias = new();

    [ObservableProperty]
    private ObservableCollection<SubTipo> _subtipos = new();

    [ObservableProperty]
    private ObservableCollection<AbaLancamento> _abas = new();

    public LancamentosViewModel(
        ITransacaoRepository transacaoRepo,
        IBancoRepository bancoRepo,
        ICategoriaRepository categoriaRepo,
        ISubTipoRepository subTipoRepo,
        IExportadorExcel exportador,
        IFileSaveService fileSaveService,
        IDialogService dialogService)
    {
        _transacaoRepo = transacaoRepo;
        _bancoRepo = bancoRepo;
        _categoriaRepo = categoriaRepo;
        _subTipoRepo = subTipoRepo;
        _exportador = exportador;
        _fileSaveService = fileSaveService;
        _dialogService = dialogService;

        var hoje = DateTime.Today;
        DataInicio = new DateTimeOffset(hoje.Year, hoje.Month, 1, 0, 0, 0, TimeSpan.Zero);
        DataFim = DataInicio.Value.AddMonths(1).AddDays(-1);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            await LoadAuxiliaresAsync();

            if (!DataInicio.HasValue || !DataFim.HasValue)
                return;

            var inicio = DataInicio.Value.Date;
            var fim = DataFim.Value.Date.AddDays(1).AddTicks(-1);

            var transacoes = await _transacaoRepo.GetByPeriodoAsync(inicio, fim);
            Transacoes = new ObservableCollection<Transacao>(transacoes);

            var abas = new ObservableCollection<AbaLancamento>();
            foreach (var subtipo in Subtipos.OrderBy(s => s.Tipo).ThenBy(s => s.Nome))
            {
                var transacoesDoSubtipo = transacoes
                    .Where(t => t.Categoria?.SubTipoId == subtipo.Id)
                    .ToList();

                abas.Add(new AbaLancamento
                {
                    Nome = subtipo.Nome,
                    Transacoes = new ObservableCollection<Transacao>(transacoesDoSubtipo)
                });
            }
            Abas = abas;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await ShowTransacaoDialogAsync(null);
    }

    [RelayCommand]
    private async Task EditAsync(Transacao? transacao)
    {
        if (transacao == null)
        {
            await _dialogService.ShowMessageAsync("Aviso", "Nenhuma transação recebida para edição.");
            return;
        }
        await ShowTransacaoDialogAsync(transacao);
    }

    [RelayCommand]
    private async Task DeleteAsync(Transacao? transacao)
    {
        if (transacao == null)
        {
            await _dialogService.ShowMessageAsync("Aviso", "Nenhuma transação recebida para exclusão.");
            return;
        }

        bool confirmado = await _dialogService.ShowConfirmationAsync(
            "Confirmar exclusão",
            $"Tem certeza que deseja excluir a transação de {transacao.Data:dd/MM/yyyy} no valor {transacao.Valor:C}?");

        if (!confirmado)
            return;

        await _transacaoRepo.DeleteAsync(transacao);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (Abas.Count == 0)
        {
            await _dialogService.ShowMessageAsync("Exportação", "Não há dados para exportar.");
            return;
        }

        var gruposLancamentos = Abas.Select(aba => new TransacaoGrupo
        {
            Nome = aba.Nome,
            Transacoes = aba.Transacoes
        }).ToList();

        var todasTransacoes = Abas.SelectMany(a => a.Transacoes).ToList();

        var resumoBancos = new List<ResumoBancoExportItem>();

        foreach (var banco in Bancos)
        {
            var transacoesDoBanco = todasTransacoes.Where(t => t.BancoId == banco.Id).ToList();

            var pixDebito = transacoesDoBanco
                .Where(t => t.TipoPagamento == TipoPagamento.PixDebito)
                .OrderBy(t => t.Data)
                .ToList();

            var credito = transacoesDoBanco
                .Where(t => t.TipoPagamento == TipoPagamento.Credito)
                .OrderBy(t => t.Data)
                .ToList();

            resumoBancos.Add(new ResumoBancoExportItem
            {
                Banco = banco,
                PixDebitoTransacoes = pixDebito,
                CreditoTransacoes = credito
            });
        }

        string? filePath = await _fileSaveService.PickSaveFileAsync("transacoes.xlsx", "Excel Workbook", ".xlsx");
        if (string.IsNullOrEmpty(filePath))
            return;

        var bytes = await _exportador.ExportarPlanilhaCompletaAsync(gruposLancamentos, resumoBancos);
        await File.WriteAllBytesAsync(filePath, bytes);

        await _dialogService.ShowMessageAsync("Exportação", $"Arquivo salvo em:\n{filePath}");
    }

    private async Task ShowTransacaoDialogAsync(Transacao? transacaoExistente)
    {
        bool isEdicao = transacaoExistente != null;
        await LoadAuxiliaresAsync();

        // Data (dia, mês, ano)
        var diaComboBox = new ComboBox { Header = "Dia", HorizontalAlignment = HorizontalAlignment.Stretch };
        var mesComboBox = new ComboBox { Header = "Mês", HorizontalAlignment = HorizontalAlignment.Stretch };
        var anoComboBox = new ComboBox { Header = "Ano", HorizontalAlignment = HorizontalAlignment.Stretch };

        for (int d = 1; d <= 31; d++) diaComboBox.Items.Add(d);

        string[] meses = { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };
        foreach (var m in meses) mesComboBox.Items.Add(m);

        int anoAtual = DateTime.Now.Year;
        for (int a = anoAtual - 5; a <= anoAtual + 5; a++) anoComboBox.Items.Add(a);

        DateTime dataInicial = isEdicao ? transacaoExistente!.Data : DateTime.Now;
        diaComboBox.SelectedItem = dataInicial.Day;
        mesComboBox.SelectedItem = meses[dataInicial.Month - 1];
        anoComboBox.SelectedItem = dataInicial.Year;

        var dataPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        dataPanel.Children.Add(diaComboBox);
        dataPanel.Children.Add(mesComboBox);
        dataPanel.Children.Add(anoComboBox);

        // Tipo (Receita/Débito)
        var tipoComboBox = new ComboBox
        {
            Header = "Tipo",
            ItemsSource = Enum.GetValues(typeof(TipoTransacao)).Cast<TipoTransacao>().ToList(),
            SelectedItem = isEdicao ? transacaoExistente!.Tipo : TipoTransacao.Receita,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Categoria
        var categoriaComboBox = new ComboBox
        {
            Header = "Categoria",
            DisplayMemberPath = "Nome",
            PlaceholderText = "Selecione a categoria",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Banco
        var bancoComboBox = new ComboBox
        {
            Header = "Banco",
            DisplayMemberPath = "Nome",
            PlaceholderText = "Selecione o banco",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Tipo de Pagamento (Pix/Débito ou Crédito)
        var radioPixDebito = new RadioButton
        {
            Content = "Pix/Débito",
            GroupName = "TipoPagamento",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var radioCredito = new RadioButton
        {
            Content = "Crédito",
            GroupName = "TipoPagamento",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var tipoPagamentoPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            Margin = new Thickness(0, 0, 0, 8)
        };
        tipoPagamentoPanel.Children.Add(radioPixDebito);
        tipoPagamentoPanel.Children.Add(radioCredito);

        var tipoPagamentoLabel = new TextBlock
        {
            Text = "Tipo de Pagamento",
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        };

        // Valor
        var valorNumberBox = new NumberBox
        {
            Header = "Valor",
            PlaceholderText = "0,00",
            Value = isEdicao ? (double)transacaoExistente!.Valor : double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Observação
        var observacaoTextBox = new TextBox
        {
            Header = "Observação",
            Text = isEdicao ? transacaoExistente!.Observacao : string.Empty,
            PlaceholderText = "Opcional"
        };

        // Função para atualizar categorias e banco
        void AtualizarCategoriasEBanco()
        {
            if (tipoComboBox.SelectedItem is not TipoTransacao tipoSelecionado)
                return;

            var categoriasFiltradas = Categorias
                .Where(c => c.Tipo == tipoSelecionado)
                .ToList();
            categoriaComboBox.ItemsSource = categoriasFiltradas;

            if (isEdicao && transacaoExistente != null)
            {
                categoriaComboBox.SelectedItem = categoriasFiltradas
                    .FirstOrDefault(c => c.Id == transacaoExistente.CategoriaId);
            }
            else
            {
                categoriaComboBox.SelectedIndex = -1;
            }

            AtualizarEstadoBanco();
        }

        // Função para habilitar/desabilitar banco e tipo pagamento
        void AtualizarEstadoBanco()
        {
            if (categoriaComboBox.SelectedItem is Categoria categoria && categoria.SubTipo != null)
            {
                bool vincularBanco = categoria.SubTipo.VincularBanco;
                bancoComboBox.IsEnabled = vincularBanco;
                if (!vincularBanco)
                {
                    bancoComboBox.SelectedIndex = -1;
                    AtualizarTipoPagamento();
                }
                else if (isEdicao && transacaoExistente != null)
                {
                    bancoComboBox.SelectedItem = Bancos.FirstOrDefault(b => b.Id == transacaoExistente.BancoId);
                    AtualizarTipoPagamento();
                }
                else
                {
                    bancoComboBox.SelectedIndex = -1;
                    AtualizarTipoPagamento();
                }
            }
            else
            {
                bancoComboBox.IsEnabled = false;
                bancoComboBox.SelectedIndex = -1;
                AtualizarTipoPagamento();
            }
        }

        // Função para habilitar/desabilitar campo Tipo de Pagamento
        void AtualizarTipoPagamento()
        {
            bool habilitar = bancoComboBox.SelectedItem != null;
            radioPixDebito.IsEnabled = habilitar;
            radioCredito.IsEnabled = habilitar;

            if (!habilitar)
            {
                radioPixDebito.IsChecked = false;
                radioCredito.IsChecked = false;
            }
            else if (isEdicao && transacaoExistente?.TipoPagamento != null)
            {
                if (transacaoExistente.TipoPagamento == TipoPagamento.PixDebito)
                    radioPixDebito.IsChecked = true;
                else if (transacaoExistente.TipoPagamento == TipoPagamento.Credito)
                    radioCredito.IsChecked = true;
                else
                {
                    radioPixDebito.IsChecked = false;
                    radioCredito.IsChecked = false;
                }
            }
        }

        // Eventos
        tipoComboBox.SelectionChanged += (s, e) => AtualizarCategoriasEBanco();
        categoriaComboBox.SelectionChanged += (s, e) => AtualizarEstadoBanco();
        bancoComboBox.SelectionChanged += (s, e) => AtualizarTipoPagamento();

        bancoComboBox.ItemsSource = Bancos;
        AtualizarCategoriasEBanco();

        // Montagem do painel
        var panel = new StackPanel();
        panel.Children.Add(dataPanel);
        panel.Children.Add(tipoComboBox);
        panel.Children.Add(categoriaComboBox);
        panel.Children.Add(bancoComboBox);
        panel.Children.Add(tipoPagamentoLabel);
        panel.Children.Add(tipoPagamentoPanel);
        panel.Children.Add(valorNumberBox);
        panel.Children.Add(observacaoTextBox);

        var dialog = new ContentDialog
        {
            Title = isEdicao ? "Editar Transação" : "Nova Transação",
            Content = panel,
            PrimaryButtonText = "Salvar",
            CloseButtonText = "Cancelar",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        // Validações
        if (categoriaComboBox.SelectedItem is not Categoria categoriaSelecionada)
        {
            await _dialogService.ShowMessageAsync("Validação", "Selecione uma categoria.");
            return;
        }

        Banco? banco = null;
        if (categoriaSelecionada.SubTipo?.VincularBanco == true)
        {
            if (bancoComboBox.SelectedItem is not Banco bancoSel)
            {
                await _dialogService.ShowMessageAsync("Validação", "Selecione um banco.");
                return;
            }
            banco = bancoSel;

            if (radioPixDebito.IsChecked != true && radioCredito.IsChecked != true)
            {
                await _dialogService.ShowMessageAsync("Validação", "Selecione o tipo de pagamento.");
                return;
            }
        }

        TipoPagamento? tipoPagamento = null;
        if (radioPixDebito.IsChecked == true)
            tipoPagamento = TipoPagamento.PixDebito;
        else if (radioCredito.IsChecked == true)
            tipoPagamento = TipoPagamento.Credito;

        // Validação do valor
        decimal valor = 0;
        bool valorValido = false;

        if (valorNumberBox.Value is double valorDouble && !double.IsNaN(valorDouble) && valorDouble > 0)
        {
            valor = (decimal)valorDouble;
            valorValido = true;
        }
        else if (!string.IsNullOrWhiteSpace(valorNumberBox.Text))
        {
            if (decimal.TryParse(valorNumberBox.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out decimal valorTexto) && valorTexto > 0)
            {
                valor = valorTexto;
                valorValido = true;
            }
            else if (decimal.TryParse(valorNumberBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out valorTexto) && valorTexto > 0)
            {
                valor = valorTexto;
                valorValido = true;
            }
        }

        if (!valorValido)
        {
            await _dialogService.ShowMessageAsync("Validação", "Informe um valor válido maior que zero.");
            return;
        }

        // Montar data
        if (diaComboBox.SelectedItem is int dia &&
            mesComboBox.SelectedItem is string mesString &&
            anoComboBox.SelectedItem is int anoSelecionado)
        {
            int mes = Array.IndexOf(meses, mesString) + 1;
            DateTime dataSelecionada;
            try
            {
                dataSelecionada = new DateTime(anoSelecionado, mes, dia);
            }
            catch (ArgumentOutOfRangeException)
            {
                await _dialogService.ShowMessageAsync("Validação", "Data inválida.");
                return;
            }

            if (isEdicao)
            {
                transacaoExistente!.Data = dataSelecionada;
                transacaoExistente.Tipo = categoriaSelecionada.Tipo;
                transacaoExistente.Valor = valor;
                transacaoExistente.Observacao = observacaoTextBox.Text;
                transacaoExistente.CategoriaId = categoriaSelecionada.Id;
                transacaoExistente.Categoria = categoriaSelecionada;
                transacaoExistente.BancoId = banco?.Id;
                transacaoExistente.Banco = banco;
                transacaoExistente.TipoPagamento = tipoPagamento;

                await _transacaoRepo.UpdateAsync(transacaoExistente);
            }
            else
            {
                var novaTransacao = new Transacao
                {
                    Data = dataSelecionada,
                    Tipo = categoriaSelecionada.Tipo,
                    Valor = valor,
                    Observacao = observacaoTextBox.Text,
                    CategoriaId = categoriaSelecionada.Id,
                    Categoria = categoriaSelecionada,
                    BancoId = banco?.Id,
                    Banco = banco,
                    TipoPagamento = tipoPagamento
                };

                await _transacaoRepo.AddAsync(novaTransacao);
            }

            await LoadAsync();
        }
        else
        {
            await _dialogService.ShowMessageAsync("Validação", "Selecione uma data válida.");
        }
    }

    private async Task LoadAuxiliaresAsync()
    {
        if (Bancos.Count == 0)
            Bancos = new ObservableCollection<Banco>(await _bancoRepo.GetAllAsync());

        if (Categorias.Count == 0)
            Categorias = new ObservableCollection<Categoria>(await _categoriaRepo.GetAllAsync());

        if (Subtipos.Count == 0)
            Subtipos = new ObservableCollection<SubTipo>(await _subTipoRepo.GetAllAsync());
    }
}