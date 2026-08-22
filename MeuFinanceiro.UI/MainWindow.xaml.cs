using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MeuFinanceiro.UI.Pages;
using System;

namespace MeuFinanceiro.UI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        // Navegar para a página inicial
        ContentFrame.Navigate(typeof(LancamentosPage));
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item)
        {
            Type? pageType = item.Tag switch
            {
                "lancamentos" => typeof(LancamentosPage),
                "categorias" => typeof(CategoriasPage),
                "bancos" => typeof(BancosPage),
                "resumo_banco" => typeof(ResumoBancoPage),
                "resumo_mensal" => typeof(ResumoMensalPage),
                _ => null
            };

            if (pageType != null)
                ContentFrame.Navigate(pageType);
        }
    }
}