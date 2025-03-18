using System.Text;

namespace SteamToys.Contact.Model;
public class AppSetting
{
    /// <summary>
    /// 并发线程数
    /// </summary>
    public int Thread { get; set; }

    /// <summary>
    /// Steam请求重试次数
    /// </summary>
    public int RequestRetry { get; set; }

    /// <summary>
    /// Steam账户重试次数
    /// </summary>
    public int AccountRetry { get; set; }

    /// <summary>
    /// 等待验证码/验证邮件时间 单位：秒
    /// </summary>
    public int WaitCodeTime { get; set; }

    /// <summary>
    /// 是否获取报价链接
    /// </summary>
    public bool IsGetTradeoffers { get; set; }

    /// <summary>
    /// 隐私设置
    /// </summary>
    public Privacy Privacy { get; set; }

    /// <summary>
    /// 文件输出路径
    /// </summary>
    public string OutputPath { get; set; }

    /// <summary>
    /// 邮箱配置
    /// </summary>
    public EmailboxConfig EmailboxConfig { get; set; }

    /// <summary>
    /// 接码平台设置
    /// </summary>
    public SmsConfig SmsConfig { get; set; }

    /// <summary>
    /// 代理账户
    /// </summary>
    public List<Proxy> Proxies { get; set; }

    public string InitOutputPath()
    {
        var path = string.IsNullOrWhiteSpace(OutputPath) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".Export") : OutputPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;

    }

    public string GetConfigInfoString(int accountCount, AppSetting config)
    {
        var requestRetry = config.RequestRetry;
        var accountRetry = config.AccountRetry;
        var waitCode = config.WaitCodeTime;
        var isGetTradeoffers = config.IsGetTradeoffers;
        var privacy = config.Privacy;
        var smsPlatform = (SmsPlatform)config.SmsConfig.Platform;
        var smsCountry = SmsConst.GetSmsCountryOption(smsPlatform, config.SmsConfig.Country);
        var smsService = SmsConst.GetSmsServiceOption(smsPlatform, config.SmsConfig.Service);

        var emailboxPrefix = config.EmailboxConfig.Prefix;
        var emailboxPort = config.EmailboxConfig.Port;
        var emailboxIsSSL = config.EmailboxConfig.IsSsl;
        var emailboxProtocol = (EmailboxProto)config.EmailboxConfig.Protocol;
        var isCustomDomain = config.EmailboxConfig.IsCustomDomain;
        var customDomain = config.EmailboxConfig.CustomDomain ?? string.Empty;

        var proxies = config.Proxies;

        var sb = new StringBuilder();
        sb.AppendLine($"开始绑定任务 - 账号总数：【{accountCount}】");
        sb.AppendLine($"基本配置信息 - 线程数：【{config.Thread}】；重试次数：【{config.RequestRetry}】；获取报价链接：【{(isGetTradeoffers ? "启用" : "关闭")}】");
        sb.AppendLine($"隐私配置信息 - 好友：【{config.Privacy.Inventory.GetDescription()}】；库存：【{config.Privacy.Inventory.GetDescription()}】；礼物：【{config.Privacy.Inventory.GetDescription()}】；游戏：【{config.Privacy.OwnedGames.GetDescription()}】；游戏时间：【{config.Privacy.Inventory.GetDescription()}】");
        sb.AppendLine($"接码平台信息 - 接码平台：【{smsPlatform.GetDescription()}】；国家：【{smsCountry.Name}】；项目：【{smsService.Name}】");
        sb.AppendLine($"邮箱配置信息 - 邮箱协议：【{emailboxProtocol.GetDescription()}】；端口：【{emailboxPort}】；SSL：【{(emailboxIsSSL ? "启用" : "关闭")}】；{(isCustomDomain ? $"自定义域名：【{customDomain}】" : $"前缀：【{emailboxPrefix}】")}；");
        sb.AppendLine($"-----------------------------------------------------------");

        return sb.ToString();
    }
}

public class EmailboxConfig
{
    /// <summary>
    /// 邮箱协议
    /// </summary>
    public int Protocol { get; set; }

    /// <summary>
    /// 邮箱服务地址前缀
    /// </summary>
    public string Prefix { get; set; }

    /// <summary>
    /// 邮箱服务端口
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 是否SSL
    /// </summary>
    public bool IsSsl { get; set; }

    /// <summary>
    /// 是否自定义域名
    /// </summary>
    public bool IsCustomDomain { get; set; }

    /// <summary>
    /// 自定义域名
    /// </summary>
    public string? CustomDomain { get; set; }
}

public class SmsConfig
{
    /// <summary>
    /// 平台
    /// </summary>
    public int Platform { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    public string Country { get; set; }

    /// <summary>
    /// 项目
    /// </summary>
    public int Service { get; set; }

    /// <summary>
    /// 电话号码长度
    /// </summary>
    public int Length { get; set; }

    /// <summary>
    /// OnlineSim接码平台Key
    /// </summary>
    public string OnlineSimApiKey { get; set; }

    /// <summary>
    /// 5-Sim接码平台Key
    /// </summary>
    public string FiveSimApiKey { get; set; }

    /// <summary>
    /// SmsActivate接码平台Key
    /// </summary>
    public string SmsActivateApiKey { get; set; }
}

public class Privacy
{
    public bool IsSetPrivacy { get; set; }

    /// <summary>
    /// 好友列表
    /// </summary>
    public PrivacyType FriendsList { get; set; }

    /// <summary>
    /// 库存
    /// </summary>
    public PrivacyType Inventory { get; set; }

    /// <summary>
    /// 礼物
    /// </summary>
    public PrivacyType InventoryGifts { get; set; }

    /// <summary>
    /// 游戏详情
    /// </summary>
    public PrivacyType OwnedGames { get; set; }

    /// <summary>
    /// 游戏时间
    /// </summary>
    public PrivacyType Playtime { get; set; }

    /// <summary>
    /// 获取个人隐私项
    /// </summary>
    /// <returns></returns>
    public PrivacyType GetPrivacyProfile()
    {
        var list = new List<PrivacyType>
        {
            FriendsList,
            Inventory,
            InventoryGifts,
            OwnedGames,
            Playtime
        };
        if (list.Any(c => c == PrivacyType.Private)) return PrivacyType.Private;
        else if (list.Any(c => c == PrivacyType.Public)) return PrivacyType.Public;
        else if (list.Any(c => c == PrivacyType.FriendsOnly)) return PrivacyType.FriendsOnly;
        return PrivacyType.None;
    }
}