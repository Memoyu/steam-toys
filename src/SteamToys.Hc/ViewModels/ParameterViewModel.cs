using HandyControl.Controls;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using SteamToys.Contact.Model;
using SteamToys.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamToys.ViewModels;

public partial class ParameterViewModel : ObservableObject, INavigationAware
{
    bool _dataInitialized = false;

    [ObservableProperty] int threadCountMax = 20;
    [ObservableProperty] string totalWaitCodeTime = "共0秒";
    [ObservableProperty] IEnumerable<BaseOption<PrivacyType>> privacyTypes = new BaseOption<PrivacyType>[] { };

    [ObservableProperty] IEnumerable<BaseOption> smsPlatforms = new BaseOption[] { };
    [ObservableProperty] IEnumerable<SmsSeviceOption> smsServices = SmsConst.SMS_ACTIVATE_SERVICIES;
    [ObservableProperty] IEnumerable<SmsCountryOption> smsCountries = new SmsCountryOption[] { };

    [ObservableProperty] IEnumerable<BaseOption> emailboxProtos = new BaseOption[] { };
    [ObservableProperty] IEnumerable<BaseOption> proxyTypes = new BaseOption[] { };

    [ObservableProperty] string proxyStr = string.Empty;

    [ObservableProperty] AppSettingViewModel appSettingVm;

    private AppSetting _appSetting;
    private readonly ILogger _logger;

    public ParameterViewModel(ILoggerFactory loggerFactory, IOptions<AppSetting> options)
    {
        _logger = loggerFactory.CreateLogger<ParameterViewModel>();
        _appSetting = options.Value;
    }

    [RelayCommand]
    async Task SmsPlaformSelectionChanged()
    {
        var platform = (SmsPlatform)AppSettingVm.SmsConfig.Platform;
        SmsCountries = GetCountryOptions(platform);
        var firstCountry = SmsConst.GetSmsCountryOption(platform);
        AppSettingVm.SmsConfig.Country = firstCountry.Country;
        // AppSettingVm.SmsConfig.Country = null;
        await Task.CompletedTask;
    }

    private IEnumerable<SmsCountryOption> GetCountryOptions(SmsPlatform platform)
    {
        return SmsConst.GetSmsCountryOptions(platform).OrderBy(c => c.Name).ToList();
    }

    [RelayCommand]
    async Task SmsServiceSelectionChanged()
    {
        await Task.CompletedTask;
    }

    [RelayCommand]
    async Task SmsCountrySelectionChanged()
    {
        await Task.CompletedTask;
    }


    [RelayCommand]
    void CopyPathClick()
    {
        Clipboard.SetText(AppSettingVm.OutputPath);
    }

    [RelayCommand]
    void SelectedPathClick()
    {
        var dialog = new VistaFolderBrowserDialog();
        dialog.Description = "选择输出文件夹";
        dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
        var result = dialog.ShowDialog();
        if (result.HasValue && result.Value)
        {
            AppSettingVm.OutputPath = dialog.SelectedPath;
        };
    }

    [RelayCommand]
    void AnalyzeProxyClick()
    {
        if (string.IsNullOrWhiteSpace(ProxyStr))
        {
            Growl.Warning("代理内容为空，请确认");
            return;
        }

        var items = new List<ProxyConfigViewModel>();
        try
        {
            var lines = ProxyStr.SplitByLine();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var props = line.Split(":");
                var ip = props[0];
                var port = props[1];
                if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(port)) continue;
                var proxy = new ProxyConfigViewModel
                {
                    Ip = ip,
                    Port = int.Parse(port),
                    Username = props.Length >= 3 ? props[2] : string.Empty,
                    Password = props.Length >= 4 ? props[3] : string.Empty,
                    ProxyType = props.Length >= 5 ? int.Parse(props[4]) : ProxyType.Socks5.GetHashCode(),
                };

                items.Add(proxy);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析代理数据失败");
            Growl.Warning("代理格式有误，请确认");
            return;
        }

        AppSettingVm.Proxies = items;
        Growl.Info($"解析成功，共{items.Count}");
    }

    [RelayCommand]
    void DeleteProxyClick()
    {
        AppSettingVm.Proxies = new List<ProxyConfigViewModel> { };
        Growl.Info($"清除成功");
    }

    [RelayCommand]
    void SaveParameterClick()
    {
        try
        {
            if (AppSettingVm.WaitCodeTime > 30)
            {
                Growl.Warning("验证等待次数过大，请设置小于30！");
                return;
            }

            if (AppSettingVm.Thread > ThreadCountMax)
            {
                Growl.Warning($"并发线程数过大，请设置小于{ThreadCountMax}！");
                return;
            }

            if (string.IsNullOrWhiteSpace(AppSettingVm.SmsConfig.Country))
            {
                Growl.Warning($"请选择短信平台国家");
                return;
            }

            AppSettingVm.Adapt(_appSetting);
            var json = JsonConvert.SerializeObject(_appSetting);

            //项目根目录
            string contentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + @"\";
            var filePath = contentPath + "appsettings.json";
            if (!File.Exists(filePath))
            {
                //创建配置文件
                FileStream fs = File.Create(filePath);
                fs.Close();
            }

            JObject jsonObject;
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            using (StreamReader file = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                jsonObject = (JObject)JToken.ReadFrom(reader);
            }
            using (var writer = new StreamWriter(filePath))
            using (JsonTextWriter jsonwriter = new JsonTextWriter(writer))
            {
                jsonwriter.Formatting = Formatting.Indented;
                jsonwriter.Indentation = 2;
                jsonwriter.IndentChar = ' ';
                jsonObject.WriteTo(jsonwriter);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置数据失败");
            Growl.Error("保存配置失败");
            return;
        }
        Growl.Info("保存成功");
    }

    [RelayCommand]
    async Task WitCodeTimeTextChanged()
    {
        var time = AppSettingVm.WaitCodeTime;
        if (time < 0)
        {
            Growl.Warning("请输入有效数值");
            return;
        }

        var total = 0;
        var sum = 0;
        for (int i = 1; i <= time; i++)
        {
            sum += i;
            if (sum >= 30) sum = 30;
            total += sum;
        }


        TotalWaitCodeTime = $"共{total}秒";
        await Task.CompletedTask;
    }

    private async void InitializeData()
    {
        // 初始化线程最大数
        ThreadCountMax = Process.GetCurrentProcess().Threads.Count;

        var privacyTypes = new List<BaseOption<PrivacyType>>();
        foreach (PrivacyType item in Enum.GetValues(typeof(PrivacyType)))
        {
            privacyTypes.Add(new BaseOption<PrivacyType>
            {
                Id = item,
                Name = item.GetDescription(),
            });
        }
        PrivacyTypes = privacyTypes;

        var paltforms = new List<BaseOption>();
        foreach (SmsPlatform item in Enum.GetValues(typeof(SmsPlatform)))
        {
            paltforms.Add(new BaseOption
            {
                Id = Convert.ToInt32(item),
                Name = item.GetDescription(),
            });
        }
        SmsPlatforms = paltforms;

        // await SmsPlaformSelectionChanged();

        var protos = new List<BaseOption>();
        foreach (EmailboxProto item in Enum.GetValues(typeof(EmailboxProto)))
        {
            protos.Add(new BaseOption
            {
                Id = Convert.ToInt32(item),
                Name = item.GetDescription(),
            });
        }
        EmailboxProtos = protos;

        var proxytypes = new List<BaseOption>();
        foreach (ProxyType item in Enum.GetValues(typeof(ProxyType)))
        {
            proxytypes.Add(new BaseOption
            {
                Id = Convert.ToInt32(item),
                Name = item.GetDescription(),
            });
        }
        ProxyTypes = proxytypes;

        var platform = (SmsPlatform)_appSetting.SmsConfig.Platform;
        SmsCountries = GetCountryOptions(platform);

        // 初始化路径
        if (string.IsNullOrWhiteSpace(_appSetting.OutputPath))
        {
            _appSetting.OutputPath = _appSetting.InitOutputPath();
        }

        AppSettingVm = _appSetting.Adapt<AppSettingViewModel>();
        await WitCodeTimeTextChanged();
        _dataInitialized = true;
    }

    public void OnNavigatedTo()
    {
        if (!_dataInitialized)
            InitializeData();
    }

    public void OnNavigatedFrom()
    {
    }
}
