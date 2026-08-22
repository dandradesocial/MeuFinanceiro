using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace MeuFinanceiro.UI.Services;

public class FileSaveService : IFileSaveService
{
    public async Task<string?> PickSaveFileAsync(string defaultFileName, string fileTypeDescription, string extension)
    {
        var savePicker = new FileSavePicker();
        savePicker.SuggestedFileName = defaultFileName;
        savePicker.FileTypeChoices.Add(fileTypeDescription, new[] { extension });

        // Obter a janela principal para a caixa de diálogo
        var window = App.MainWindow;
        if (window == null) return null;
        var hwnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(savePicker, hwnd);

        StorageFile file = await savePicker.PickSaveFileAsync();
        return file?.Path;
    }
}