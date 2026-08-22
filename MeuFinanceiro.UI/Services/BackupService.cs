using System;
using System.IO;
using System.Threading.Tasks;

namespace MeuFinanceiro.UI.Services;

public class BackupService : IBackupService
{
    private readonly string _dbPath;
    private readonly string _backupDirectory;

    public BackupService()
    {
        _dbPath = Path.Combine(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
            "financeiro.db");
        _backupDirectory = Path.Combine(
            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
            "Backup");
    }

    public async Task BackupAsync()
    {
        if (!File.Exists(_dbPath))
            return;

        Directory.CreateDirectory(_backupDirectory);
        string backupPath = Path.Combine(_backupDirectory, $"financeiro_{DateTime.Now:yyyyMMdd_HHmmss}.db");
        await Task.Run(() => File.Copy(_dbPath, backupPath, overwrite: true));
    }
}