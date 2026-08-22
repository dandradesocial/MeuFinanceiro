using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeuFinanceiro.Core.Entities;
using MeuFinanceiro.Core.Interfaces;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Text;

namespace MeuFinanceiro.UI.ViewModels;

public partial class BancosViewModel : ObservableObject
{
    private readonly IBancoRepository _bancoRepo;

    [ObservableProperty]
    private ObservableCollection<Banco> _bancos = new();

    [ObservableProperty]
    private Banco? _bancoSelecionado;

    public BancosViewModel(IBancoRepository bancoRepo)
    {
        _bancoRepo = bancoRepo;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        var bancos = await _bancoRepo.GetAllAsync();
        Bancos = new ObservableCollection<Banco>(bancos);
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        await ShowBancoDialogAsync(null);
    }

    [RelayCommand]
    private async Task EditAsync(Banco? banco)
    {
        if (banco == null) return;
        await ShowBancoDialogAsync(banco);
    }

    [RelayCommand]
    private async Task DeleteAsync(Banco? banco)
    {
        if (banco == null) return;

        var dialog = new ContentDialog
        {
            Title = "Confirmar exclusão",
            Content = $"Tem certeza que deseja excluir o banco \"{banco.Nome}\"?",
            PrimaryButtonText = "Excluir",
            CloseButtonText = "Cancelar",
            XamlRoot = App.MainWindow?.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
            return;

        await _bancoRepo.DeleteAsync(banco);
        await LoadAsync();
    }

    private async Task ShowBancoDialogAsync(Banco? bancoExistente)
    {
        bool isEdicao = bancoExistente != null;

        // Campo de nome
        var nomeTextBox = new TextBox
        {
            Header = "Nome",
            PlaceholderText = "Ex.: Banco do Brasil",
            Text = isEdicao ? bancoExistente!.Nome : string.Empty
        };

        // Rótulo para o seletor de cor (já que ColorPicker não tem Header)
        var corLabel = new TextBlock
        {
            Text = "Cor",
            FontWeight = FontWeights.SemiBold,   // Usar FontWeights sem qualificação
            Margin = new Thickness(0, 0, 0, 4)
        };

        // Seletor de cor (ColorPicker)
        var colorPicker = new ColorPicker
        {
            IsAlphaEnabled = true,                // Permite canal alfa (ARGB)
            IsAlphaSliderVisible = true,          // Mostra slider de alpha
            IsAlphaTextInputVisible = true,       // Mostra campo numérico para alpha
            ColorSpectrumShape = ColorSpectrumShape.Ring,
            IsMoreButtonVisible = true,
            IsColorChannelTextInputVisible = true,
            IsColorSliderVisible = true,
            IsHexInputVisible = true,
            Color = isEdicao ? ConvertHexToColor(bancoExistente!.CorHex) : Colors.Black
        };

        // TextBlock para erros inline
        var erroTextBlock = new TextBlock
        {
            Foreground = new SolidColorBrush(Colors.Red),
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var panel = new StackPanel();
        panel.Children.Add(nomeTextBox);
        panel.Children.Add(corLabel);
        panel.Children.Add(colorPicker);
        panel.Children.Add(erroTextBlock);

        var dialog = new ContentDialog
        {
            Title = isEdicao ? "Editar Banco" : "Novo Banco",
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

        dialog.PrimaryButtonClick += async (s, e) =>
        {
            var deferral = e.GetDeferral();
            try
            {
                // Validação do nome
                if (string.IsNullOrWhiteSpace(nomeTextBox.Text))
                {
                    ExibirErro("O nome do banco é obrigatório.");
                    e.Cancel = true;
                    return;
                }

                string nome = nomeTextBox.Text.Trim();
                bool nomeDuplicado = Bancos.Any(b =>
                    b.Id != (isEdicao ? bancoExistente!.Id : (Guid?)null) &&
                    string.Equals(b.Nome.Trim(), nome, StringComparison.OrdinalIgnoreCase));

                if (nomeDuplicado)
                {
                    ExibirErro("Já existe um banco com esse nome.");
                    e.Cancel = true;
                    return;
                }

                // A cor é obrigatória; o ColorPicker sempre fornece um Color válido.
                Color cor = colorPicker.Color;
                string corHex = ConvertColorToHex(cor);

                // Persistência
                if (isEdicao)
                {
                    bancoExistente!.Nome = nome;
                    bancoExistente.CorHex = corHex;
                    await _bancoRepo.UpdateAsync(bancoExistente);
                }
                else
                {
                    var novoBanco = new Banco
                    {
                        Nome = nome,
                        CorHex = corHex
                    };
                    await _bancoRepo.AddAsync(novoBanco);
                }

                await LoadAsync();
            }
            catch (Exception ex)
            {
                ExibirErro($"Ocorreu um erro ao salvar: {ex.Message}");
                e.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        };

        await dialog.ShowAsync();
    }

    // ===== Funções auxiliares para conversão de cor =====

    private static Color ConvertHexToColor(string hex)
    {
        // Remove o '#' se existir
        hex = hex.Replace("#", string.Empty);

        byte a = 255, r = 0, g = 0, b = 0;

        try
        {
            if (hex.Length == 8) // ARGB
            {
                a = Convert.ToByte(hex.Substring(0, 2), 16);
                r = Convert.ToByte(hex.Substring(2, 2), 16);
                g = Convert.ToByte(hex.Substring(4, 2), 16);
                b = Convert.ToByte(hex.Substring(6, 2), 16);
            }
            else if (hex.Length == 6) // RGB (sem alpha)
            {
                r = Convert.ToByte(hex.Substring(0, 2), 16);
                g = Convert.ToByte(hex.Substring(2, 2), 16);
                b = Convert.ToByte(hex.Substring(4, 2), 16);
            }
            else
            {
                return Colors.Black;
            }
        }
        catch
        {
            return Colors.Black;
        }

        return Color.FromArgb(a, r, g, b);
    }

    private static string ConvertColorToHex(Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}