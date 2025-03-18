using Microsoft.Extensions.Options;
using System.Text;

namespace SteamToys.Service.Sms;

public class OnlineSimProvider : ISmsProvider
{
    private readonly ILogger<OnlineSimProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly AppSetting _config;
    public SmsPlatform Platform => SmsPlatform.OnlineSim;

    public OnlineSimProvider(ILoggerFactory loggerFactory, IOptions<AppSetting> options, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<OnlineSimProvider>();
        _httpClient = httpClientFactory.CreateClient("online-sim");
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
                {"service",service }, {"country",country }
            });
            respStr = await _httpClient.GetStringAsync($"getNum.php{query}");
            var resp = JsonConvert.DeserializeObject<GetNumOnlineSimResp>(respStr);
            if (resp == null || resp.Response == "INTERVAL_CONCURRENT_REQUESTS_ERROR") return result;

            if (resp?.Response != "1") throw new Exception(respStr);
            result.Id = resp.Tzid;
            var stateResp = (await GetStateAsync(resp.Tzid))?.FirstOrDefault();
            if (stateResp is null || string.IsNullOrWhiteSpace(stateResp.Number)) throw new Exception($"获取号码状态失败 响应：{JsonConvert.SerializeObject(stateResp)}");
            if (stateResp.Number.Length != length) throw new Exception($"电话号码长度不满配置要求 响应：{JsonConvert.SerializeObject(stateResp)}");

            result.PhoneNumber = stateResp.Number;
        }
        catch (Exception ex)
        {
            result.IsRetry = true;
            _logger.LogError(ex, $"OnlineSim获取电话号码异常 resp: {respStr}");
        }

        return result;
    }

    public async Task<GetPhoneNumberStatusResponse> GetPhoneNumberStatusAsync(string id)
    {
        var result = new GetPhoneNumberStatusResponse();
        try
        {
            // 获取长度为5的验证码
            var resp = (await GetStateAsync(id))?.FirstOrDefault(s => s.Msg?.Length == 5);
            if (string.IsNullOrWhiteSpace(resp?.Msg)) result.IsRetry = true;
            result.Code = resp?.Msg;
            result.Status = resp?.Response;
        }
        catch (Exception)
        {
            result.IsRetry = true;
        }
        return result;
    }

    public async Task<bool> DestroyPhoneNumberAsync(string id)
    {
        var respStr = string.Empty;
        try
        {
            var query = GetQuery(new Dictionary<string, string>
            {
                {"tzid",id }
            });
            respStr = await _httpClient.GetStringAsync($"setOperationOk.php{query}");
            var resp = JsonConvert.DeserializeObject<GetNumOnlineSimResp>(respStr);
            _logger.LogInformation($"销毁电话号码响应 resp: {respStr}");
            if (resp == null || resp.Response == "NO_COMPLETE_TZID") throw new Exception($"销毁电话号码错误");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"OnlineSim销毁电话号码异常 resp: {respStr}");
            return false;
        }
    }

    private async Task<List<GetNumStateOnlineSimResp>> GetStateAsync(string tzid)
    {
        var respStr = string.Empty;
        try
        {
            var query = GetQuery(new Dictionary<string, string>
            {
                {"tzid",tzid }, {"msg_list", "0" }, {"clean","1" }, {"message_to_code","1" }
            });
            respStr = await _httpClient.GetStringAsync($"getState.php{query}");
            var resp = JsonConvert.DeserializeObject<List<GetNumStateOnlineSimResp>>(respStr);
            return resp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"OnlineSim获取电话号码状态失败 tzid: {tzid}, resp: {respStr}");
            throw;
        }
    }

    private string GetQuery(Dictionary<string, string> parameters)
    {
        var apiKey = _config?.SmsConfig?.OnlineSimApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new FriendlyException("Online-Sim 配置为空，确认配置文件是否正确");
        var sb = new StringBuilder();
        sb.Append($"?apikey={apiKey}");
        foreach (var p in parameters)
        {
            sb.Append($"&{p.Key}={p.Value}");
        }

        return sb.ToString();
    }
}
