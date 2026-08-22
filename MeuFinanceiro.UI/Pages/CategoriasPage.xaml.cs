using CommunityToolkit.WinUI.UI.Controls;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.Pages;

public sealed partial class CategoriasPage : Page
{
    public CategoriasViewModel ViewModel { get; }

    public CategoriasPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<CategoriasViewModel>();
        this.DataContext = ViewModel;
        _ = ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        var clickedElement = e.OriginalSource as DependencyObject;
        if (clickedElement == null) return;

        var categoria = FindCategoria(clickedElement) ?? ViewModel.CategoriaSelecionada;
        if (categoria == null) return;

        var menu = new MenuFlyout();

        var editarItem = new MenuFlyoutItem { Text = "Editar" };
        editarItem.Click += async (s, args) =>
        {
            try
            {
                await ViewModel.EditCommand.ExecuteAsync(categoria);
            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        };

        var excluirItem = new MenuFlyoutItem { Text = "Excluir" };
        excluirItem.Click += async (s, args) =>
        {
            try
            {
                await ViewModel.DeleteCommand.ExecuteAsync(categoria);
            }
            catch (Exception ex)
            {
                await ShowError(ex.Message);
            }
        };

        menu.Items.Add(editarItem);
        menu.Items.Add(excluirItem);

        if (sender is FrameworkElement element)
        {
            menu.ShowAt(element, e.GetPosition(element));
        }
    }

    // ===== Gerenciar Subtipos =====
    private async void OnGerenciarSubtiposClick(object sender, RoutedEventArgs e)
    {
        await ShowSubtiposDialogAsync();
    }

    private async Task ShowSubtiposDialogAsync()
    {
        // Garantir que a lista de subtipos esteja carregada
        if (ViewModel.Subtipos.Count == 0)
            await ViewModel.LoadCommand.ExecuteAsync(null);

        // Criar DataGrid para subtipos
        var subtiposGrid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true,
            IsRightTapEnabled = false,
            ItemsSource = ViewModel.Subtipos,
            MinHeight = 200,
            MaxHeight = 400
        };

        subtiposGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Nome",
            Binding = new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Nome") }
        });
        subtiposGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "Tipo",
            Binding = new Microsoft.UI.Xaml.Data.Binding { Path = new PropertyPath("Tipo") }
        });

        // Declara o diálogo antes de usá-lo nos handlers
        ContentDialog? subtiposDialog = null;

        subtiposGrid.RightTapped += (s, e) =>
        {
            var clickedElement = e.OriginalSource as DependencyObject;
            if (clickedElement == null) return;

            var subtipo = FindSubtipo(clickedElement);
            if (subtipo == null) return;

            var menu = new MenuFlyout();

            var editarItem = new MenuFlyoutItem { Text = "Editar" };
            editarItem.Click += async (sender, args) =>
            {
                subtiposDialog?.Hide();          // Fecha o diálogo antes de abrir outro
                await Task.Delay(100);

                try
                {
                    await ViewModel.EditSubTipoCommand.ExecuteAsync(subtipo);
                }
                catch (Exception ex)
                {
                    await ShowError(ex.Message);
                }
            };

            var excluirItem = new MenuFlyoutItem { Text = "Excluir" };
            excluirItem.Click += async (sender, args) =>
            {
                subtiposDialog?.Hide();          // Fecha o diálogo antes de abrir outro
                await Task.Delay(100);

                try
                {
                    await ViewModel.DeleteSubTipoCommand.ExecuteAsync(subtipo);
                }
                catch (Exception ex)
                {
                    await ShowError(ex.Message);
                }
            };

            menu.Items.Add(editarItem);
            menu.Items.Add(excluirItem);

            if (s is FrameworkElement element)
            {
                menu.ShowAt(element, e.GetPosition(element));
            }
        };

        // Painel do diálogo
        var panel = new StackPanel();
        panel.Children.Add(subtiposGrid);

        subtiposDialog = new ContentDialog
        {
            Title = "Subtipos",
            Content = panel,
            PrimaryButtonText = "Fechar",
            CloseButtonText = null,
            XamlRoot = this.XamlRoot
        };

        await subtiposDialog.ShowAsync();
    }

    // ===== Funções auxiliares =====

    private Categoria? FindCategoria(DependencyObject? child)
    {
        while (child != null)
        {
            if (child is FrameworkElement fe && fe.DataContext is Categoria categoria)
                return categoria;

            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private SubTipo? FindSubtipo(DependencyObject? child)
    {
        while (child != null)
        {
            if (child is FrameworkElement fe && fe.DataContext is SubTipo subtipo)
                return subtipo;

            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private async Task ShowError(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Erro",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}