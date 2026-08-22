using System.Threading.Tasks;

namespace MeuFinanceiro.UI.Services;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string message);
    Task<bool> ShowConfirmationAsync(string title, string message);
}