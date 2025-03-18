namespace SteamToys.Service;

public interface IWorkService
{
    Task DoWorkWithRetryAsync(AppSetting config, List<SteamAccount> accounts, CancellationToken token);
}
