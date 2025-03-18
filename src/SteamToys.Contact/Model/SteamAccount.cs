using SteamToys.Core.Excel.Attributes;

namespace SteamToys.Contact.Model;

public class SteamAccount : ObservableObject
{
    private bool isSelect;
    public bool IsSelect { get => isSelect; set => SetProperty(ref isSelect, value); }

    [HeaderCell("序号")]
    public int Id { get; set; }

    [HeaderCell("账号")]
    public string Steam { get; set; }

    [HeaderCell("密码")]
    public string SteamPassword { get; set; }

    [HeaderCell("邮箱")]
    public string Email { get; set; }

    [HeaderCell("邮箱密码")]
    public string EmailPassword { get; set; }

    private string smsPlatform;
    [HeaderCell("短信平台")]
    public string SmsPlatform { get => smsPlatform; set => SetProperty(ref smsPlatform, value); }

    private string phoneNumber;
    [HeaderCell("手机号码")]
    public string PhoneNumber { get => phoneNumber; set => SetProperty(ref phoneNumber, value); }

    private string cptcha;
    [HeaderCell("验证码")]
    public string Captcha { get => cptcha; set => SetProperty(ref cptcha, value); }

    private string emailCode;
    public string EmailCode { get => emailCode; set => SetProperty(ref emailCode, value); }

    private string recoverCode;
    [HeaderCell("恢复码")]
    public string RecoverCode { get => recoverCode; set => SetProperty(ref recoverCode, value); }

    private string quoteUrl;
    [HeaderCell("报价链接")]
    public string QuoteUrl { get => quoteUrl; set => SetProperty(ref quoteUrl, value); }

    private string privacyInventory;
    [HeaderCell("库存状态")]
    public string PrivacyInventory { get => privacyInventory; set => SetProperty(ref privacyInventory, value); }

    private string bindStatus;
    [HeaderCell("绑定状态")]
    public string BindStatus { get => bindStatus; set => SetProperty(ref bindStatus, value); }

    private string bindDateTime;
    [HeaderCell("绑定时间")]
    public string BindDateTime { get => bindDateTime; set => SetProperty(ref bindDateTime, value); }

    private string errMessage;
    [HeaderCell("错误日志")]
    public string ErrMessage { get => errMessage; set => SetProperty(ref errMessage, value); }

    public int RetryLoginCount { get; set; }

    public int RetryBindCount { get; set; }

    public void Info(string message)
    {
        ErrMessage = message;
    }

    public void Fail(string message)
    {
        ErrMessage = message;
        BindStatus = "失败";
        BindDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public BindError BindError { get; set; }

    public void Init()
    {
        SmsPlatform = string.Empty;
        PhoneNumber = string.Empty;
        Captcha = string.Empty;
        EmailCode = string.Empty;
        RecoverCode = string.Empty;
        QuoteUrl = string.Empty;
        PrivacyInventory = string.Empty;
        BindStatus = string.Empty;
        BindDateTime = string.Empty;
        ErrMessage = string.Empty;
    }
}
