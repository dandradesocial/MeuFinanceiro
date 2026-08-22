using MeuFinanceiro.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MeuFinanceiro.UI.Pages;

public sealed partial class ResumoMensalPage : Page
{
    public ResumoMensalViewModel ViewModel { get; }

    public ResumoMensalPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ResumoMensalViewModel>();
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