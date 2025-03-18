using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Serilog.Parsing;
using System.Text;

namespace SteamToys.Service.Sms;

public class FiveSimProvider : ISmsProvider
{
    private readonly ILogger<FiveSimProvider> _logger;
    private readonly HttpClient _httpClient;
    private readonly AppSetting _config;
    public SmsPlatform Platform => SmsPlatform.FiveSim;

    public FiveSimProvider(ILoggerFactory loggerFactory, IOptions<AppSetting> options, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<FiveSimProvider>();
        _httpClient = httpClientFactory.CreateClient("5-sim");
        _config = options.Value;
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task<GetPhoneNumberResponse> GetPhoneNumberAsync(string service, string country)
    {
        var result = new GetPhoneNumberResponse();
        var respStr = string.Empty;
        var length = _config?.SmsConfig?.Length ?? 0;
        try
        {
            SetToken(_httpClient);

            var prices = from p in await GetProductPricesAsync(service, country)
                         where p.Count > 0
                         orderby p.Cost, p.Count descending, p.Rate
                         select p;
            var @operator = prices.FirstOrDefault()?.Operator ?? "any";

            respStr = await _httpClient.GetStringAsync($"v1/user/buy/activation/{country}/{@operator}/{service}");
            var resp = JsonConvert.DeserializeObject<GetNumFiveSimResp>(respStr);
            if (string.IsNullOrWhiteSpace(resp?.Phone)) throw new Exception(respStr);
            if (resp.Phone.Length < length) throw new Exception($"电话号码长度不满配置要求 响应：{respStr}");
            result.Id = resp.Id.ToString();
            result.PhoneNumber = resp.Phone;
        }
        catch (Exception ex)
        {
            result.IsRetry = true;
            _logger.LogError(ex, $"5-Sim 获取电话号码失败 resp: {respStr}");
        }
        return result;
    }

    public async Task<GetPhoneNumberStatusResponse> GetPhoneNumberStatusAsync(string id)
    {
        var result = new GetPhoneNumberStatusResponse();
        var respStr = string.Empty;
        try
        {
            SetToken(_httpClient);
            respStr = await _httpClient.GetStringAsync($"v1/user/check/{id}");
            var resp = JsonConvert.DeserializeObject<GetNumFiveSimResp>(respStr);
            if (string.IsNullOrWhiteSpace(resp?.Phone)) throw new Exception(respStr);
            var sms = resp.Sms?.FirstOrDefault();
            result.Code = sms?.Code;
            if (string.IsNullOrWhiteSpace(result.Code)) throw new Exception(respStr);
            result.Status = resp.Status;
        }
        catch (Exception ex)
        {
            result.IsRetry = true;
            _logger.LogError(ex, $"5-Sim 获取电话号码状态失败 resp: {respStr}");
        }
        return result;
    }

    public async Task<bool> DestroyPhoneNumberAsync(string id)
    {
        var result = false;
        var respStr = string.Empty;
        try
        {
            SetToken(_httpClient);
            respStr = await _httpClient.GetStringAsync($"v1/user/cancel/{id}");
            var resp = JsonConvert.DeserializeObject<GetNumFiveSimResp>(respStr);
            return resp?.Status == "CANCELED";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"5-Sim 取消订单失败 resp: {respStr}");
        }
        return result;
    }

    public async Task<List<FiveSimProductPrice>> GetProductPricesAsync(string service, string country)
    {
        var result = new List<FiveSimProductPrice>();
        var respStr = string.Empty;
        try
        {
            SetToken(_httpClient);
            respStr = await _httpClient.GetStringAsync($"v1/guest/prices?country={country}&product={service}");
            JObject? jo = JsonConvert.DeserializeObject(respStr) as JObject;
            if (jo == null) return result;
            IEnumerable<JProperty>? properties = jo[country]?[service]?.Value<JObject>()?.Properties();
            if (properties == null) return result;
            foreach (JProperty prop in properties)
            {
                var price = JsonConvert.DeserializeObject<FiveSimProductPrice>(prop.Value.ToString());
                if (price == null) continue;
                price.Operator = prop.Name;
                result.Add(price);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"5-Sim 获取{country}, {service}价格列表失败 resp: {respStr}");
        }
        return result;
    }

    private void SetToken(HttpClient httpClient)
    {
        var token = _config?.SmsConfig?.FiveSimApiKey;
        if (string.IsNullOrWhiteSpace(token))
            throw new FriendlyException("5-Sim 配置为空，确认配置文件是否正确");
        httpClient.DefaultRequestHeaders.Remove("Authorization");

        httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
    }
}
