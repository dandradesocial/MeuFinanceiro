using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.Services;

public class DialogService : IDialogService
{
    public async Task ShowMessageAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };
        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Sim",
            CloseButtonText = "Não",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}