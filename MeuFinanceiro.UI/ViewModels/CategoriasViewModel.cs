using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Enums;
using MeuFinanceiro.Core.Interfaces;
using MeuFinanceiro.UI.Services;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.ViewModels;

public partial class CategoriasViewModel : ObservableObject
{
    private readonly ICategoriaRepository _categoriaRepo;
    private readonly ISubTipoRepository _subTipoRepo;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ObservableCollection<Categoria> _categorias = new();

    [ObservableProperty]
    private ObservableCollection<SubTipo> _subtipos = new();

    [ObservableProperty]
    private Categoria? _categoriaSelecionada;

    public CategoriasViewModel(
        ICategoriaRepository categoriaRepo,
        ISubTipoRepository subTipoRepo,
        IDialogService dialogService)
    {
        _categoriaRepo = categoriaRepo;
        _subTipoRepo = subTipoRepo;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            await CarregarCategoriasAsync();
            await CarregarSubtiposAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CarregarCategoriasAsync()
    {
        var categorias = await _categoriaRepo.GetAllAsync();
        Categorias = new ObservableCollection<Categoria>(categorias);
    }

    private async Task CarregarSubtiposAsync()
    {
        var subtipos = await _subTipoRepo.GetAllAsync();
        Subtipos = new ObservableCollection<SubTipo>(subtipos.OrderBy(s => s.Tipo).ThenBy(s => s.Nome));
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await ShowCategoriaDialogAsync(null);
    }

    [RelayCommand]
    private async Task EditAsync(Categoria? categoria)
    {
        if (categoria == null) return;
        await ShowCategoriaDialogAsync(categoria);
    }

    [RelayCommand]
    private async Task DeleteAsync(Categoria? categoria)
    {
        if (categoria == null) return;

        bool confirmado = await _dialogService.ShowConfirmationAsync(
            "Confirmar exclusão",
            $"Tem certeza que deseja excluir a categoria \"{categoria.Nome}\"?");

        if (!confirmado)
            return;

        await _categoriaRepo.DeleteAsync(categoria);
        await LoadAsync();
    }

    [RelayCommand]
    private async Task NovoSubtipoAsync()
    {
        await ShowSubtipoDialogAsync(null);
    }

    [RelayCommand]
    private async Task EditSubTipoAsync(SubTipo? subtipo)
    {
        if (subtipo == null) return;
        await ShowSubtipoDialogAsync(subtipo);
    }

    [RelayCommand]
    private async Task DeleteSubTipoAsync(SubTipo? subtipo)
    {
        if (subtipo == null) return;

        bool confirmado = await _dialogService.ShowConfirmationAsync(
            "Confirmar exclusão",
            $"Tem certeza que deseja excluir o subtipo \"{subtipo.Nome}\"?");

        if (!confirmado)
            return;

        await _subTipoRepo.DeleteAsync(subtipo);
        await LoadAsync();
    }

    private async Task ShowSubtipoDialogAsync(SubTipo? subtipoExistente)
    {
        bool isEdicao = subtipoExistente != null;
        await CarregarSubtiposAsync();

        var nomeTextBox = new TextBox
        {
            Header = "Nome",
            PlaceholderText = "Ex.: Compras",
            Text = isEdicao ? subtipoExistente!.Nome : string.Empty
        };

        var tipoComboBox = new ComboBox
        {
            Header = "Tipo",
            ItemsSource = Enum.GetValues(typeof(TipoTransacao)).Cast<TipoTransacao>().ToList(),
            SelectedItem = isEdicao ? subtipoExistente!.Tipo : TipoTransacao.Debito,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var vincularBancoComboBox = new ComboBox
        {
            Header = "Vincular a um Banco?",
            ItemsSource = new[] { "Sim", "Não" },
            SelectedItem = isEdicao ? (subtipoExistente!.VincularBanco ? "Sim" : "Não") : "Sim",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var panel = new StackPanel();
        panel.Children.Add(nomeTextBox);
        panel.Children.Add(tipoComboBox);
        panel.Children.Add(vincularBancoComboBox);

        var dialog = new ContentDialog
        {
            Title = isEdicao ? "Editar Subtipo" : "Cadastro de Subtipo",
            Content = panel,
            PrimaryButtonText = "Salvar",
            CloseButtonText = "Cancelar",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        if (string.IsNullOrWhiteSpace(nomeTextBox.Text))
        {
            await _dialogService.ShowMessageAsync("Validação", "O nome do subtipo é obrigatório.");
            return;
        }

        var nome = nomeTextBox.Text.Trim();
        if (Subtipos.Any(s => s.Id != (isEdicao ? subtipoExistente!.Id : (Guid?)null) &&
                              string.Equals(s.Nome, nome, StringComparison.OrdinalIgnoreCase)))
        {
            await _dialogService.ShowMessageAsync("Validação", "Já existe um subtipo com esse nome.");
            return;
        }

        var novoTipo = (TipoTransacao)tipoComboBox.SelectedItem!;
        bool vincularBanco = (string)vincularBancoComboBox.SelectedItem == "Sim";

        if (isEdicao)
        {
            subtipoExistente!.Nome = nome;
            subtipoExistente.Tipo = novoTipo;
            subtipoExistente.VincularBanco = vincularBanco;
            await _subTipoRepo.UpdateAsync(subtipoExistente);
        }
        else
        {
            var novoSubTipo = new SubTipo
            {
                Nome = nome,
                Tipo = novoTipo,
                VincularBanco = vincularBanco
            };
            await _subTipoRepo.AddAsync(novoSubTipo);
        }

        await LoadAsync();
    }

    private async Task ShowCategoriaDialogAsync(Categoria? categoriaExistente)
    {
        bool isEdicao = categoriaExistente != null;
        await CarregarSubtiposAsync();

        var nomeTextBox = new TextBox
        {
            Header = "Nome",
            PlaceholderText = "Ex.: Alimentação",
            Text = isEdicao ? categoriaExistente!.Nome : string.Empty
        };

        var subtipoComboBox = new ComboBox
        {
            Header = "Subtipo",
            DisplayMemberPath = "Nome",
            ItemsSource = Subtipos,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            SelectedItem = isEdicao
                ? Subtipos.FirstOrDefault(s => s.Id == categoriaExistente!.SubTipoId)
                : Subtipos.FirstOrDefault(s => s.Tipo == TipoTransacao.Receita)
        };

        var erroTextBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.Red),
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var panel = new StackPanel();
        panel.Children.Add(nomeTextBox);
        panel.Children.Add(subtipoComboBox);
        panel.Children.Add(erroTextBlock);

        var dialog = new ContentDialog
        {
            Title = isEdicao ? "Editar Categoria" : "Nova Categoria",
            Content = panel,
            PrimaryButtonText = "Salvar",
            CloseButtonText = "Cancelar",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        void ExibirErro(string mensagem)
        {
            erroTextBlock.Text = mensagem;
            erroTextBlock.Visibility = Visibility.Visible;
        }

        void LimparErro()
        {
            erroTextBlock.Text = string.Empty;
            erroTextBlock.Visibility = Visibility.Collapsed;
        }

        nomeTextBox.TextChanged += (s, e) => LimparErro();
        subtipoComboBox.SelectionChanged += (s, e) => LimparErro();

        dialog.PrimaryButtonClick += async (s, e) =>
        {
            var deferral = e.GetDeferral();
            try
            {
                if (string.IsNullOrWhiteSpace(nomeTextBox.Text))
                {
                    ExibirErro("O nome da categoria é obrigatório.");
                    e.Cancel = true;
                    return;
                }

                if (subtipoComboBox.SelectedItem is not SubTipo subtipoSelecionado)
                {
                    ExibirErro("Selecione um subtipo.");
                    e.Cancel = true;
                    return;
                }

                string nome = nomeTextBox.Text.Trim();
                bool nomeDuplicado = Categorias.Any(c =>
                    c.Id != (isEdicao ? categoriaExistente!.Id : (Guid?)null) &&
                    c.Tipo == subtipoSelecionado.Tipo &&
                    string.Equals(c.Nome.Trim(), nome, StringComparison.OrdinalIgnoreCase));

                if (nomeDuplicado)
                {
                    ExibirErro($"Já existe uma categoria de {subtipoSelecionado.Tipo} com esse nome.");
                    e.Cancel = true;
                    return;
                }

                if (isEdicao)
                {
                    categoriaExistente!.Nome = nome;
                    categoriaExistente.Tipo = subtipoSelecionado.Tipo;
                    categoriaExistente.SubTipoId = subtipoSelecionado.Id;
                    categoriaExistente.SubTipo = subtipoSelecionado;
                    await _categoriaRepo.UpdateAsync(categoriaExistente);
                }
                else
                {
                    var novaCategoria = new Categoria
                    {
                        Nome = nome,
                        Tipo = subtipoSelecionado.Tipo,
                        SubTipoId = subtipoSelecionado.Id,
                        SubTipo = subtipoSelecionado
                    };
                    await _categoriaRepo.AddAsync(novaCategoria);
                }

                await LoadAsync();
            }
            catch (Exception ex)
            {
                ExibirErro($"Erro ao salvar: {ex.Message}");
                e.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }
}