using System.Threading.Tasks;

namespace MeuFinanceiro.UI.Services;

public interface IFileSaveService
{
    Task<string?> PickSaveFileAsync(string defaultFileName, string fileTypeDescription, string extension);
}