using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SteamToys.Contact.Model;

/// <summary>
/// Steam 账号实体
/// </summary>
public class SteamGuardAccount
{
    private static byte[] steamGuardCodeTranslations = new byte[] { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };

    [JsonProperty("shared_secret")]
    public string SharedSecret { get; set; }

    [JsonProperty("serial_number")]
    public string SerialNumber { get; set; }

    [JsonProperty("revocation_code")]
    public string RevocationCode { get; set; }

    [JsonProperty("uri")]
    public string URI { get; set; }

    [JsonProperty("server_time")]
    public long ServerTime { get; set; }

    [JsonProperty("account_name")]
    public string AccountName { get; set; }

    [JsonProperty("token_gid")]
    public string TokenGID { get; set; }

    [JsonProperty("identity_secret")]
    public string IdentitySecret { get; set; }

    [JsonProperty("secret_1")]
    public string Secret1 { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("device_id")]
    public string DeviceID { get; set; } = GenerateDeviceID();

    /// <summary>
    /// 如果身份验证器已实际应用于该帐户，则设置为true。
    /// </summary>
    [JsonProperty("fully_enrolled")]
    public bool FullyEnrolled { get; set; }

    public SessionData Session { get; set; }

    public string GenerateSteamGuardCode()
    {
        return GenerateSteamGuardCodeForTime(GetSteamTimeAsync().GetAwaiter().GetResult());
    }

    public string GenerateSteamGuardCodeForTime(long time)
    {
        if (SharedSecret == null || SharedSecret.Length == 0)
        {
            return "";
        }

        string sharedSecretUnescaped = Regex.Unescape(SharedSecret);
        byte[] sharedSecretArray = Convert.FromBase64String(sharedSecretUnescaped);
        byte[] timeArray = new byte[8];

        time /= 30L;

        for (int i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte)time;
            time >>= 8;
        }

        HMACSHA1 hmacGenerator = new HMACSHA1();
        hmacGenerator.Key = sharedSecretArray;
        byte[] hashedData = hmacGenerator.ComputeHash(timeArray);
        byte[] codeArray = new byte[5];
        try
        {
            byte b = (byte)(hashedData[19] & 0xF);
            int codePoint = (hashedData[b] & 0x7F) << 24 | (hashedData[b + 1] & 0xFF) << 16 | (hashedData[b + 2] & 0xFF) << 8 | (hashedData[b + 3] & 0xFF);

            for (int i = 0; i < 5; ++i)
            {
                codeArray[i] = steamGuardCodeTranslations[codePoint % steamGuardCodeTranslations.Length];
                codePoint /= steamGuardCodeTranslations.Length;
            }
        }
        catch (Exception)
        {
            return null; //Change later, catch-alls are bad!
        }
        return Encoding.UTF8.GetString(codeArray);
    }

    private static bool _aligned = false;
    private static int _timeDifference = 0;


    public static async Task<long> GetSteamTimeAsync()
    {
        if (!_aligned)
        {
            await AlignTimeAsync();
        }
        return Util.GetSystemUnixTime() + _timeDifference;
    }


    public static async Task AlignTimeAsync()
    {
        long currentTime = Util.GetSystemUnixTime();
        WebClient client = new WebClient();
        try
        {
            string response = await client.UploadStringTaskAsync(new Uri(APIEndpoints.TWO_FACTOR_TIME_QUERY), "steamid=0");
            TimeQuery query = JsonConvert.DeserializeObject<TimeQuery>(response);
            _timeDifference = (int)(query.Response.ServerTime - currentTime);
            _aligned = true;
        }
        catch (WebException)
        {
            return;
        }
    }

    public static string GenerateDeviceID()
    {
        return "android:" + Guid.NewGuid().ToString();
    }
}
