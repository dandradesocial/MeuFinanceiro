using CommunityToolkit.WinUI.UI.Controls;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;

namespace MeuFinanceiro.UI.Pages;

public sealed partial class BancosPage : Page
{
    public BancosViewModel ViewModel { get; }

    public BancosPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<BancosViewModel>();
        this.DataContext = ViewModel;
        _ = ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void DataGrid_RightTapped(object sender, RightTappedRoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        var originalSource = e.OriginalSource as DependencyObject;
        var row = FindParent<DataGridRow>(originalSource);
        if (row == null) return;

        if (row.DataContext is not Banco banco) return;

        var menu = new MenuFlyout();

        var editarItem = new MenuFlyoutItem { Text = "Editar" };
        editarItem.Click += async (s, args) =>
        {
            await ViewModel.EditCommand.ExecuteAsync(banco);
        };

        var excluirItem = new MenuFlyoutItem { Text = "Excluir" };
        excluirItem.Click += async (s, args) =>
        {
            await ViewModel.DeleteCommand.ExecuteAsync(banco);
        };

        menu.Items.Add(editarItem);
        menu.Items.Add(excluirItem);

        menu.ShowAt(element, e.GetPosition(element));
    }

    private static T? FindParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }
}