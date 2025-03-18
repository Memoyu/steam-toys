namespace SteamToys.Contact.Model;
public class AppSettingViewModel : ObservableObject
{
    /// <summary>
    /// 并发线程数
    /// </summary>
    private int thread;
    public int Thread { get => thread; set => SetProperty(ref thread, value); }

    /// <summary>
    /// Steam请求重试次数
    /// </summary>
    private int requestRetry;
    public int RequestRetry { get => requestRetry; set => SetProperty(ref requestRetry, value); }

    /// <summary>
    /// Steam账户请求重试次数
    /// </summary>
    private int accountRetry;
    public int AccountRetry { get => accountRetry; set => SetProperty(ref accountRetry, value); }

    /// <summary>
    /// 等待验证码/验证邮件时间 单位：秒
    /// </summary>
    private int waitCodeTime;
    public int WaitCodeTime { get => waitCodeTime; set => SetProperty(ref waitCodeTime, value); }

    /// <summary>
    /// 是否获取报价链接
    /// </summary>
    private bool isGetTradeoffers;
    public bool IsGetTradeoffers { get => isGetTradeoffers; set => SetProperty(ref isGetTradeoffers, value); }

    /// <summary>
    /// 隐私设置
    /// </summary>
    private PrivacyViewModel privacy;
    public PrivacyViewModel Privacy { get => privacy; set => SetProperty(ref privacy, value); }

    /// <summary>
    /// 文件输出路径
    /// </summary>
    private string outputPath;
    public string OutputPath { get => outputPath; set => SetProperty(ref outputPath, value); }

    /// <summary>
    /// 邮箱配置
    /// </summary>
    public EmailboxConfigViewModel EmailboxConfig { get; set; }

    /// <summary>
    /// 接码平台设置
    /// </summary>
    private SmsConfigViewModel smsConfig;
    public SmsConfigViewModel SmsConfig { get => smsConfig; set => SetProperty(ref smsConfig, value); }

    //public ProxyConfigViewModel ProxyConfig { get; set; }

    /// <summary>
    /// 代理账户
    /// </summary>
    private List<ProxyConfigViewModel> proxies = new List<ProxyConfigViewModel>();
    public List<ProxyConfigViewModel> Proxies { get => proxies; set => SetProperty(ref proxies, value); }
}

public class EmailboxConfigViewModel : ObservableObject
{
    /// <summary>
    /// 邮箱协议
    /// </summary>
    private int protocol;
    public int Protocol { get => protocol; set => SetProperty(ref protocol, value); }

    /// <summary>
    /// 邮箱服务地址前缀
    /// </summary>
    private string? prefix;
    public string Prefix { get => prefix; set => SetProperty(ref prefix, value); }

    /// <summary>
    /// 邮箱服务端口
    /// </summary>
    private int port;
    public int Port { get => port; set => SetProperty(ref port, value); }

    /// <summary>
    /// 是否SSL
    /// </summary>
    private bool isSsl;
    public bool IsSsl { get => isSsl; set => SetProperty(ref isSsl, value); }

    /// <summary>
    /// 是否自定义域名
    /// </summary>
    private bool isCustomDomain;
    public bool IsCustomDomain { get => isCustomDomain; set => SetProperty(ref isCustomDomain, value); }

    /// <summary>
    /// 自定义域名
    /// </summary>
    private string? customDomain;
    public string CustomDomain { get => customDomain; set => SetProperty(ref customDomain, value); }


}

public class SmsConfigViewModel : ObservableObject
{
    /// <summary>
    /// 平台
    /// </summary>
    private int platform;
    public int Platform { get => platform; set => SetProperty(ref platform, value); }

    /// <summary>
    /// 国家
    /// </summary>
    private string country;
    public string Country { get => country; set => SetProperty(ref country, value); }

    /// <summary>
    /// 项目
    /// </summary>
    private int service;
    public int Service { get => service; set => SetProperty(ref service, value); }

    /// <summary>
    /// 长度
    /// </summary>
    private int length;
    public int Length { get => length; set => SetProperty(ref length, value); }

    /// <summary>
    /// OnlineSim接码平台Key
    /// </summary>
    private string onlineSimApiKey;
    public string OnlineSimApiKey { get => onlineSimApiKey; set => SetProperty(ref onlineSimApiKey, value); }

    /// <summary>
    /// 5-Sim接码平台Key
    /// </summary>
    private string fiveSimApiKey;
    public string FiveSimApiKey { get => fiveSimApiKey; set => SetProperty(ref fiveSimApiKey, value); }

    /// <summary>
    /// SmsActivate接码平台Key
    /// </summary>
    private string smsActivateApiKey;
    public string SmsActivateApiKey { get => smsActivateApiKey; set => SetProperty(ref smsActivateApiKey, value); }
}

public class PrivacyViewModel : ObservableObject
{
    private bool isSetPrivacy;
    public bool IsSetPrivacy { get => isSetPrivacy; set => SetProperty(ref isSetPrivacy, value); }

    private PrivacyType friendsList;
    public PrivacyType FriendsList { get => friendsList; set => SetProperty(ref friendsList, value); }

    private PrivacyType inventory;
    public PrivacyType Inventory { get => inventory; set => SetProperty(ref inventory, value); }

    public PrivacyType inventoryGifts;
    public PrivacyType InventoryGifts { get => inventoryGifts; set => SetProperty(ref inventoryGifts, value); }

    public PrivacyType ownedGames;
    public PrivacyType OwnedGames { get => ownedGames; set => SetProperty(ref ownedGames, value); }

    public PrivacyType playtime;
    public PrivacyType Playtime { get => playtime; set => SetProperty(ref playtime, value); }
}

public class ProxyConfigViewModel : ObservableObject
{
    public string Ip { get; set; }

    public int Port { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    /// <summary>
    /// 代理协议
    /// </summary>
    private int proxyType;
    public int ProxyType { get => proxyType; set => SetProperty(ref proxyType, value); }
}
