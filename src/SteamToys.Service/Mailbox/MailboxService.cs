using MimeKit;
using System;

namespace SteamToys.Service.MailBox;

public class MailboxService : IMailboxService
{
    private const int TIMEOUT = 20 * 1000; // 20s , 单位：ms

    private readonly ILogger _logger;

    public MailboxService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<MailboxService>();
    }

    public async Task<string> GetLoginCodeRamblerAsync(EmailboxOption option, int waitTime, DateTime bindTime)
    {
        var res = await Policy.HandleResult<string>(code => string.IsNullOrWhiteSpace(code)).WaitAndRetryAsync(Util.GetIncreaseTimespans(waitTime), (result, sleep, count, context) =>
        {
            _logger.LogError($"获取邮箱登录验证码 result:{result.Result} 重试延时：{sleep.TotalSeconds}s-重试次数：{count}");
        }).ExecuteAsync(async () =>
        {
            var code = string.Empty;
            await ReadMimeMessageAsync(option, async (message) =>
            {
                if (message is null) return false;
                code = ParseAndGetCode(message, bindTime);
                if (string.IsNullOrEmpty(code)) return false;
                await Task.CompletedTask;
                return true;
            });

            return code;

        });

        return res;
    }

    public async Task<bool> ReadEmailAndConfirmationAsync(EmailboxOption option, int waitTime, DateTime bindTime, Func<string, Task<string>> toSteamConfirmPhoneFunc)
    {
        // 等邮件发送.....
        var res = await Policy.HandleResult<bool>(r => !r).WaitAndRetryAsync(Util.GetIncreaseTimespans(waitTime), (result, sleep, count, context) =>
         {
             _logger.LogError($"处理邮箱验证失败 result:{result.Result} 重试延时：{sleep.TotalSeconds}s-重试次数：{count}");
         }).ExecuteAsync(async () =>
         {
             var res = false;
             await ReadMimeMessageAsync(option, async (message) =>
             {
                 if (message is null) return false;
                 res = await ParseAndConfirmAsync(message, bindTime, toSteamConfirmPhoneFunc);
                 return res;
             });

             return res;
         });

        return res;
    }

    private async Task ReadMimeMessageAsync(EmailboxOption option, Func<MimeMessage, Task<bool>> func)
    {
        switch (option.Proto)
        {
            case EmailboxProto.IMAP:
                await GetByIMAPAsync();
                break;

            case EmailboxProto.POP3:
                await GetByPOP3Async();
                break;

            default: throw new Exception("未支持邮箱协议");
        }

        async Task GetByIMAPAsync()
        {
            using (var client = new ImapClient())
            {
                client.Timeout = TIMEOUT;
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(option.Domain, option.Port, option.IsSSL);
                await client.AuthenticateAsync(option.Email, option.Password);
                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);
                if (inbox.Count > 0)
                {
                    for (var i = inbox.Count - 1; i >= 0; i--)
                    {
                        var mimeMessage = await inbox.GetMessageAsync(i);
                        var isBreak = await func.Invoke(mimeMessage);
                        if (isBreak) break;
                    }
                }

                client.Disconnect(true);
            }
        }

        async Task GetByPOP3Async()
        {
            using (var client = new Pop3Client())
            {
                client.Timeout = TIMEOUT;
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(option.Domain, option.Port, option.IsSSL);
                await client.AuthenticateAsync(option.Email, option.Password);
                if (client.Count > 0)
                {
                    for (var i = client.Count - 1; i >= 0; i--)
                    {
                        var mimeMessage = await client.GetMessageAsync(i);
                        var isBreak = await func.Invoke(mimeMessage);
                        if (isBreak) break;
                    }
                }

                client.Disconnect(true);
            }
        }
    }

    private DateTime? GetEmailDateTime(MimeMessage message)
    {
        var headers = message.Headers;
        var dateHeader = headers?.FirstOrDefault(h => h.Field == "Date");
        var isParse = DateTime.TryParse(dateHeader?.Value, out var date);
        return isParse ? date : null;
    }

    private async Task<bool> ParseAndConfirmAsync(MimeMessage message, DateTime bindTime, Func<string, Task<string>> toSteamConfirmPhoneFunc)
    {
        try
        {
            var date = GetEmailDateTime(message);
            if (date is not null && date >= bindTime)
            {
                var link = Regex.Match(message.HtmlBody, "store([.])steampowered([.])com([\\/])phone([\\/])ConfirmEmailForAdd([?])stoken=([^\"]+)").Groups[0].Value;
                if (string.IsNullOrEmpty(link))
                {
                    _logger.LogInformation("邮件确认失败");
                    return false;
                }

                var resp = await toSteamConfirmPhoneFunc("https://" + link);
                if (!resp.Contains("Email Confirmed"))
                {
                    _logger.LogInformation("邮件确认失败");
                    return false;
                }
                return true;
            }
            else
            {
                _logger.LogInformation("未收到确认绑定邮件");
                return false;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "确认绑定邮件异常");
            throw;
        }
    }

    private string ParseAndGetCode(MimeMessage message, DateTime bindPhoneDate)
    {
        try
        {
            var date = GetEmailDateTime(message);
            if (date is not null && date >= bindPhoneDate)
            {
                var code = Regex.Match(message.HtmlBody, "class=([\"])title-48 c-blue1 fw-b a-center([^>]+)([>])([^<]+)").Groups[4].Value;
                code = code.Trim();
                return code;
            }
            else
            {
                _logger.LogInformation("未收到验证码邮件");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取验证码邮件异常");
            throw;
        }
    }

}
