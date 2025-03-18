using HandyControl.Controls;
using SteamToys.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SteamToys.ViewModels;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{
    bool _dataInitialized = false;

    [ObservableProperty] int completedTotal = 0;
    [ObservableProperty] int failedTotal = 0;
    [ObservableProperty] int smsCodeFailedTotal = 0;
    [ObservableProperty] int emailboxFailedTotal = 0;
    [ObservableProperty] string outLogs = string.Empty;
    [ObservableProperty] IEnumerable<SteamAccount> accounts = new SteamAccount[] { };

    private CancellationTokenSource tokenSource = new CancellationTokenSource();

    private readonly ILogger _logger;
    private AppSetting _appConfig;
    private readonly IWorkService _workService;


    public DashboardViewModel(
        ILoggerFactory loggerFactory,
        IOptions<AppSetting> options,
        IWorkService workService)
    {
        _logger = loggerFactory.CreateLogger<MainWindowViewModel>();
        _appConfig = options.Value;
        _workService = workService;
    }

    [RelayCommand]
    async Task ImportAccountClick()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog()
        {
            Filter = "Text documents (.txt)|*.txt|All files (*.*)|*.*"
        };
        var result = openFileDialog.ShowDialog();

        if (result.Value)
        {
            WriteToLog("正在加载账号信息");
            Stopwatch sw = Stopwatch.StartNew();

            // 读取文本文件内容
            var path = openFileDialog.FileName;

            var items = new List<SteamAccount>();
            try
            {
                var lines = File.ReadLines(path); ;
                if (!lines.Any())
                {
                    Growl.Warning("文件内容为空，确认后再导入");
                    return;
                }

                // 读取已经绑定成功的账号列表
                string filePath = Path.Combine(Environment.CurrentDirectory, "succeed.txt");
                var successDatas = Util.ReadSucceedDatas(filePath);
                var index = 1;
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var source = line.Trim().Split(',');
                    if (source.Length < 4) continue;
                    var steam = source[0];
                    var steamPassword = source[1];
                    var email = source[2];
                    var emailPassword = source[3];
                    if (string.IsNullOrWhiteSpace(steam) ||
                        string.IsNullOrWhiteSpace(steamPassword) ||
                        string.IsNullOrWhiteSpace(email) ||
                        string.IsNullOrWhiteSpace(emailPassword)) continue;

                    // 过滤已经成功的账号数据
                    if (successDatas.Contains(steam)) continue;
                    var account = new SteamAccount
                    {
                        Id = index++,
                        Steam = steam,
                        SteamPassword = steamPassword,
                        Email = email,
                        EmailPassword = emailPassword,
                        ErrMessage = "-",
                        BindStatus = ""
                    };
                    items.Add(account);
                }
                sw.Stop();
                WriteToLog($"账号加载完成 耗时：{sw.Elapsed.TotalSeconds}s 账号总数：{items.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入账号数据失败");
                return;
            }
            Accounts = items;

            Growl.Info($"导入成功，共{items.Count}条");
        }
        else
        {
            Growl.Warning("取消导入");
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    async Task StartClick()
    {
        if (!Accounts.Any())
        {
            Growl.Warning("列表中无账号信息，请先导入账户！");
            return;
        }

        var selects = Accounts.Where(a => a.IsSelect).ToList();
        if (!selects.Any())
        {
            Growl.Warning("列表中无选中账号信息，请先选中账户！");
            return;
        }

        var outputPath = _appConfig.InitOutputPath();
        var configString = _appConfig.GetConfigInfoString(selects.Count, _appConfig);
        WriteToLog(configString);

        tokenSource = new CancellationTokenSource();

        // 开始批处理
        await _workService.DoWorkWithRetryAsync(_appConfig, selects, tokenSource.Token);

        // 汇总
        FailedTotal = selects.Count(a => a.BindStatus == "失败");
        CompletedTotal = selects.Count - FailedTotal;
        SmsCodeFailedTotal = selects.Count(a => a.BindError == BindError.BadSmsCode);
        EmailboxFailedTotal = selects.Count(a => a.BindError == BindError.BadEmailVerify);

        try
        {
            ExcelUtil.ExportReport(Accounts.Where(m => !string.IsNullOrEmpty(m.BindStatus)).ToList(), outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"导出批处理结果失败 msg:{ex.Message}");
        }

        WriteToLog($"结束绑定任务 - 账号总数：【{selects.Count()}】；完成：【{CompletedTotal}】；失败：【{FailedTotal}】；邮箱验证失败：【{EmailboxFailedTotal}】；短信验证失败：【{SmsCodeFailedTotal}】");
        WriteToLog($"结果输出路径 - 路径：【{outputPath}】");
        WriteToLog($"-----------------------------------------------------------");
    }

    [RelayCommand]
    void ExportClick()
    {
        var outputPath = _appConfig.OutputPath;
        ExcelUtil.ExportReport(Accounts.Where(m => !string.IsNullOrEmpty(m.BindStatus)).ToList(), outputPath);
        WriteToLog($"结果输出路径 - 路径：【{outputPath}】");
    }

    [RelayCommand]
    void StopClick()
    {
        tokenSource.Cancel();
    }

    [RelayCommand]
    void DataGridSelectAllChecked(RoutedEventArgs e)
    {
        var cb = (CheckBox)e.Source;
        if (cb is null || !cb.IsChecked.HasValue) return;
        foreach (var account in Accounts)
        {
            account.IsSelect = cb.IsChecked.Value;
        }
    }


    [RelayCommand]
    void DataGridRowSelectionChanged(SelectionChangedEventArgs e)
    {
        var dataGrid = (DataGrid)e.Source;
        if (dataGrid?.CurrentColumn?.DisplayIndex == 0) return;
        if (e.AddedItems.Count == 0) return;
        var accounts = e.AddedItems.Cast<SteamAccount>();
        foreach (SteamAccount account in accounts)
        {
            var frist = Accounts.FirstOrDefault(a => account.Id == a.Id);
            if (frist is not null)
            {
                frist.IsSelect = true;
            }
        }
    }

    private void WriteToLog(string message)
    {
        byte[] array = Encoding.UTF8.GetBytes(message);
        using (MemoryStream stream = new MemoryStream(array))
        {
            using (var sr = new StreamReader(stream))
            {
                string? line = sr.ReadLine();
                while (line != null)
                {
                    OutLogs = $">> {DateTime.Now:F} -> {line}";
                    line = sr.ReadLine();
                };
            }
        }
    }

    public void OnNavigatedTo()
    {
    }

    public void OnNavigatedFrom()
    {
    }

}
