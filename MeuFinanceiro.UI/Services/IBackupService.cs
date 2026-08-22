using System.Threading.Tasks;

namespace MeuFinanceiro.UI.Services;

public interface IBackupService
{
    Task BackupAsync();
}