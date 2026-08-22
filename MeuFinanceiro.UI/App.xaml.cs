using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using MeuFinanceiro.Infrastructure;
using MeuFinanceiro.Infrastructure.Data;
using MeuFinanceiro.UI.Services;
using MeuFinanceiro.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // Configura logging
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging(builder =>
        {
            builder.AddDebug();
        });

        string dbPath = Path.Combine(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
            "financeiro.db");

        serviceCollection.AddInfrastructure($"Data Source={dbPath}");

        serviceCollection.AddSingleton<IFileSaveService, FileSaveService>();
        serviceCollection.AddSingleton<IDialogService, DialogService>();
        serviceCollection.AddSingleton<IBackupService, BackupService>();

        serviceCollection.AddTransient<LancamentosViewModel>();
        serviceCollection.AddTransient<CategoriasViewModel>();
        serviceCollection.AddTransient<BancosViewModel>();
        serviceCollection.AddTransient<ResumoBancoViewModel>();
        serviceCollection.AddTransient<ResumoMensalViewModel>();
        serviceCollection.AddSingleton<MainWindow>();

        Services = serviceCollection.BuildServiceProvider();

        // Tratamento global de exceções
        UnhandledException += OnUnhandledException;

        // Inicialização do banco com migrações
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<FinanceContext>();
            context.Database.Migrate(); // usa migrações
        }

        MainWindow = Services.GetRequiredService<MainWindow>();
        MainWindow.Activate();
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Log do erro
        var logger = Services.GetRequiredService<ILogger<App>>();
        logger.LogError(e.Exception, "Erro não tratado");

        // Exibe mensagem amigável
        var dialogService = Services.GetRequiredService<IDialogService>();
        _ = dialogService.ShowMessageAsync("Erro", "Ocorreu um erro inesperado. Por favor, tente novamente.");
        e.Handled = true;
    }
}