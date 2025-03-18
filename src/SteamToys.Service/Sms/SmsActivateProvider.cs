using ProtoBuf.Meta;
using System.Text;

namespace SteamToys.Service.Sms;

public class SmsActivateProvider : ISmsProvider
{
    private readonly ILogger<SmsActivateProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly AppSetting _config;
    public SmsPlatform Platform => SmsPlatform.SmsActivate;

    public SmsActivateProvider(ILoggerFactory loggerFactory, IOptions<AppSetting> options, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<SmsActivateProvider>();
        _httpClient = httpClientFactory.CreateClient("sms-activate");
        _config = options.Value;
    }

    public async Task<GetPhoneNumberResponse> GetPhoneNumberAsync(string service, string country)
    {
        var result = new GetPhoneNumberResponse();
        var respStr = string.Empty;
        var length = _config?.SmsConfig?.Length ?? 0;
        try
        {
            var query = GetQuery(new Dictionary<string, string>
            {
                {"action","getNumberV2" }, {"service",service }, {"country",country }
            });
            respStr = await _httpClient.GetStringAsync($"handler_api.php{query}");
            var resp = JsonConvert.DeserializeObject<GetNumSmsActivateResp>(respStr);
            if (string.IsNullOrWhiteSpace(resp?.PhoneNumber)) throw new Exception(respStr);
            if (result.PhoneNumber.Length < length) throw new Exception($"电话号码长度不满配置要求 响应：{respStr}");
            result.Id = resp.ActivationId.ToString();
            result.PhoneNumber = resp.PhoneNumber;

        }
        catch (Exception ex)
        {
            result.IsRetry = true;
            _logger.LogError(ex, $"SmsActivate 获取电话号码失败 resp: {respStr}");
        }
        return result;
    }

    public async Task<GetPhoneNumberStatusResponse> GetPhoneNumberStatusAsync(string id)
    {
        // 有验证码的情况下响应为"STATUS_OK:59198"
        var result = new GetPhoneNumberStatusResponse();
        var respStr = string.Empty;
        try
        {
            var query = GetQuery(new Dictionary<string, string>
            {
                {"action","getStatus" }, {"id",id }
            });
            respStr = await _httpClient.GetStringAsync($"handler_api.php{query}");
            if (respStr == "STATUS_WAIT_CODE" || respStr == "STATUS_WAIT_RESEND" || respStr == "STATUS_WAIT_RETRY") throw new SmsException(respStr);
            if (string.IsNullOrWhiteSpace(respStr)) throw new Exception(respStr);
            var sp = respStr.Split(':');
            if (sp.Length < 2)
            {
                result.Status = sp[0];
            }
            else
            {
                result.Code = sp[1];
                result.Status = sp[0];
            }
            if (string.IsNullOrWhiteSpace(result.Code)) throw new Exception(respStr);
        }
        catch (Exception ex)
        {
            result.IsRetry = true;
            _logger.LogError(ex, $"SmsActivate 获取电话号码状态失败 resp: {respStr}");
        }

        return result;
    }

    public Task<bool> DestroyPhoneNumberAsync(string id)
    {
        throw new NotImplementedException();
    }

    private string GetQuery(Dictionary<string, string> parameters)
    {
        var apiKey = _config?.SmsConfig?.SmsActivateApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new FriendlyException("SmsActivate 配置为空，确认配置文件是否正确");
        var sb = new StringBuilder();
        sb.Append($"?api_key={apiKey}");
        foreach (var p in parameters)
        {
            sb.Append($"&{p.Key}={p.Value}");
        }

        return sb.ToString();
    }
}
