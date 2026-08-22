using MeuFinanceiro.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MeuFinanceiro.UI.Pages;

public sealed partial class ResumoBancoPage : Page
{
    public ResumoBancoViewModel ViewModel { get; }

    public ResumoBancoPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ResumoBancoViewModel>();
        this.DataContext = ViewModel;

        dataInicioPicker.Date = ViewModel.DataInicio;
        dataFimPicker.Date = ViewModel.DataFim;

        dataInicioPicker.DateChanged += (s, e) =>
        {
            ViewModel.DataInicio = dataInicioPicker.Date;
        };
        dataFimPicker.DateChanged += (s, e) =>
        {
            ViewModel.DataFim = dataFimPicker.Date;
        };

        _ = ViewModel.LoadCommand.ExecuteAsync(null);
    }
}