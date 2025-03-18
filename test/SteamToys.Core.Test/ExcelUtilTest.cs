using SteamToys.Contact.Model;
using SteamToys.Core.Excel;

namespace SteamToys.Core.Test;

public class ExcelUtilTest
{
    [Fact]
    public void ExportReport_Test()
    {
        // 删除文件
        var file = Path.Combine(Environment.CurrentDirectory, ".test.xlsx");
        if (File.Exists(Path.GetFullPath(file)))
        {
            File.Delete(Path.GetFullPath(file));
        }

        // 构建测试数据
        var datas = new List<SteamAccount>
        {
            new SteamAccount
            {
                Id = 1,
                Steam = "steam",
                SteamPassword = "steam_password",
                Email = "email",
                EmailPassword = "email_password",
                SmsPlatform="OnlineSms",
                PhoneNumber="12345678909",
                Captcha = "445566",
                RecoverCode="RECODE23",
                QuoteUrl="http://steam.com/steam",
                PrivacyInventory = "默认",
                BindStatus="成功",
                BindDateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ErrMessage="绑定成功"
            },
            new SteamAccount
            {
                Id = 2,
                Steam = "steam2",
                SteamPassword = "steam_password2",
                Email = "email2",
                EmailPassword = "email_password2",
                SmsPlatform="OnlineSms",
                PhoneNumber="12345678908",
                Captcha = "445533",
                RecoverCode="RECODE24",
                QuoteUrl="http://steam.com/steam2",
                PrivacyInventory = "私密",
                BindStatus="失败",
                BindDateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ErrMessage="获取短信验证码失败"
            },
            new SteamAccount
            {
                Id = 3,
                Steam = "steam3",
                SteamPassword = "steam_password3",
                Email = "email3",
                EmailPassword = "email_password3",
                SmsPlatform="OnlineSms",
                PhoneNumber="12345678907",
                Captcha = "445544",
                RecoverCode="RECODE25",
                QuoteUrl="http://steam.com/steam3",
                PrivacyInventory = "私密",
                BindStatus="失败",
                BindDateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                ErrMessage="确认手机号码绑定邮件失败"
            },
        };

        // 写入文件
        ExcelUtil.ExportReport(datas, file);
        Assert.True(File.Exists(Path.GetFullPath(file)));
    }
}
