using System.Diagnostics;

namespace SteamToys.Wpfui.ViewModels;

public partial class DashboardViewModel : ObservableObject, INavigationAware
{

    bool _dataInitialized = false;
    [ObservableProperty] bool isAllSelect = true;
    [ObservableProperty] int completedTotal = 0;
    [ObservableProperty] int failedTotal = 0;
    [ObservableProperty] int smsCodeFailedTotal = 0;
    [ObservableProperty] int emailboxFailedTotal = 0;
    [ObservableProperty] string outLogs = string.Empty;
    [ObservableProperty] IEnumerable<SteamAccount> accounts = new SteamAccount[] { };
    [ObservableProperty] ConcurrentQueue<SteamAccount> queues = new ConcurrentQueue<SteamAccount>();
    CancellationTokenSource tokenSource = new CancellationTokenSource();


    private readonly ILogger _logger;
    private AppSetting _appConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly IFileService _fileService;
    private readonly ICustomSnackbarService _snackbarService;
    private readonly ILoggerFactory _loggerFactory;


    public DashboardViewModel(
        ILoggerFactory loggerFactory,
        IOptions<AppSetting> options,
        IServiceProvider serviceProvider,
        ICustomSnackbarService snackbarService,
        IFileService fileService)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DashboardViewModel>();
        _serviceProvider = serviceProvider;
        _snackbarService = snackbarService;
        _fileService = fileService;
        _appConfig = options.Value;
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
                    _snackbarService.Warning("文件内容为空，确认后再导入");
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
                        IsSelect = true,
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

            _snackbarService.Info($"导入成功，共{items.Count}条");
        }
        else
        {
            _snackbarService.Warning("取消导入");
        }

        await Task.CompletedTask;
    }

    [RelayCommand]
    async Task StartClick()
    {
        if (!Accounts.Any())
        {
            _snackbarService.Warning("列表中无账号信息，请先导入账户！");
            return;
        }

        var selects = Accounts.Where(a => a.IsSelect).ToList();
        if (!selects.Any())
        {
            _snackbarService.Warning("列表中无选中账号信息，请先选中账户！");
            return;
        }

        tokenSource = new CancellationTokenSource();

        selects.ForEach(m => Queues.Enqueue(m));
        var requestRetry = _appConfig.RequestRetry;
        var accountRetry = _appConfig.AccountRetry;
        var waitCode = _appConfig.WaitCodeTime;
        var isGetTradeoffers = _appConfig.IsGetTradeoffers;
        var privacy = _appConfig.Privacy;
        var outputPath = _appConfig.OutputPath;
        var smsPlatform = (SmsPlatform)_appConfig.SmsConfig.Platform;
        var smsCountry = SmsConst.GetSmsCountryOption(smsPlatform, _appConfig.SmsConfig.Country);
        var smsService = SmsConst.GetSmsServiceOption(smsPlatform, _appConfig.SmsConfig.Service);

        var emailboxPrefix = _appConfig.EmailboxConfig.Prefix;
        var emailboxPort = _appConfig.EmailboxConfig.Port;
        var emailboxIsSSL = _appConfig.EmailboxConfig.IsSsl;
        var emailboxProtocol = (EmailboxProto)_appConfig.EmailboxConfig.Protocol;

        var proxies = _appConfig.Proxies;

        WriteToLog($"开始绑定任务 - 账号总数：【{selects.Count()}】");
        WriteToLog($"基本配置信息 - 线程数：【{_appConfig.Thread}】；重试次数：【{_appConfig.RequestRetry}】；获取报价链接：【{(isGetTradeoffers ? "启用" : "关闭")}】");
        WriteToLog($"隐私配置信息 - 好友：【{_appConfig.Privacy.Inventory.GetDescription()}】；库存：【{_appConfig.Privacy.Inventory.GetDescription()}】；礼物：【{_appConfig.Privacy.Inventory.GetDescription()}】；游戏：【{_appConfig.Privacy.OwnedGames.GetDescription()}】；游戏时间：【{_appConfig.Privacy.Inventory.GetDescription()}】");
        WriteToLog($"接码平台信息 - 接码平台：【{smsPlatform.GetDescription()}】；国家：【{smsCountry.Name}】；项目：【{smsService.Name}】");
        WriteToLog($"邮箱配置信息 - 邮箱协议：【{emailboxProtocol.GetDescription()}】；前缀：【{emailboxPrefix}】；端口：【{emailboxPort}】；SSL：【{(emailboxIsSSL ? "启用" : "关闭")}】");

        CancellationToken cancellationToken = tokenSource.Token;

        // 跑流程
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < _appConfig.Thread; i++)
        {
            if (tokenSource.IsCancellationRequested)
            {
                // 在取消请求时清理资源或执行其他操作
                WriteToLog("任务终止了 Task cancelled!");
                break;
            }

            // 延时队列延时，错开线程
            await Task.Delay(2000);
            tasks.Add(Task.Factory.StartNew(async () =>
            {
                while (Queues.Count > 0)
                {
                    if (tokenSource.IsCancellationRequested)
                        break;
                    SteamAccount accountItem = new SteamAccount();
                    var res = Queues.TryDequeue(out accountItem);
                    if (res)
                    {
                        // 随机获取代理
                        Proxy? proxy = null;
                        if (proxies != null && proxies.Any())
                        {
                            Random rd = new Random();
                            var s = rd.Next(0, proxies.Count);
                            proxy = proxies[s];
                        }
                        var workService = _serviceProvider.GetRequiredService<IWorkService>();
                        var account = selects.FirstOrDefault(m => m.Id == accountItem.Id);
                        if (account == null) continue;
                        await workService.DoWorkWithRetryAsync(_appConfig, selects, tokenSource.Token);
                    }
                }
            }, TaskCreationOptions.LongRunning).Unwrap());
        }

        await Task.WhenAll(tasks.ToArray());
        FailedTotal = Accounts.Count(a => a.BindStatus == "失败");
        CompletedTotal = Accounts.Count() - FailedTotal;
        SmsCodeFailedTotal = Accounts.Count(a => a.BindError == BindError.BadSmsCode);
        EmailboxFailedTotal = Accounts.Count(a => a.BindError == BindError.BadEmailVerify);

        if (tokenSource.IsCancellationRequested)
            ExcelUtil.ExportReport(Accounts.Where(m => !string.IsNullOrEmpty(m.BindStatus)).ToList(), outputPath);
        else
            ExcelUtil.ExportReport(Accounts, outputPath);


        WriteToLog($"结束绑定任务 - 账号总数：【{selects.Count()}】；完成：【{CompletedTotal}】；失败：【{FailedTotal}】；邮箱验证失败：【{EmailboxFailedTotal}】；短信验证失败：【{SmsCodeFailedTotal}】");
        WriteToLog($"结果输出路径 - 路径：【{outputPath}】");
    }

    [RelayCommand]
    async Task StartClickTest()
    {
        var client = new SteamClientService(_loggerFactory);
        var loginState = new SteamLoginState
        {
            Username = "vrglq833",
            Password = "NelQ20446Y",
        };
        await client.ClientLoginAsync(loginState);
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
    void AllSelectChecked()
    {
        foreach (var account in Accounts)
        {
            account.IsSelect = IsAllSelect;
        }
    }

    private void WriteToLog(string message)
    {
        OutLogs = $">> {DateTime.Now:F} -> {message}";
    }

    public void OnNavigatedTo()
    {
        if (!_dataInitialized)
            InitializeData();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeData()
    {
        _dataInitialized = true;
    }
}
