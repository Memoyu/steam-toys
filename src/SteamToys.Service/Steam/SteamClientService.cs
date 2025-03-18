using static SteamToys.Contact.Const.APIEndpoints;
using static SteamToys.Contact.Const.UserAgent;
using System.Net;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace SteamToys.Service.Steam;

public class SteamClientService : ISteamClientService
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly CookieContainer _cookieContainer = new();
    private SteamLoginState _loginState;
    private WebProxy _webProxy;

    public SteamClientService(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<SteamClientService>();
        _client = new(new SocketsHttpHandler
        {
            UseCookies = true,
            CookieContainer = _cookieContainer,
            Proxy = _webProxy
        });
    }

    /// <summary>
    /// 等待一个时间并且重试
    /// </summary>
    /// <returns></returns>
    AsyncRetryPolicy WaitAndRetryAsync(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0) => Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(3), }, (ex, _, i, _) =>
        {
            _logger.LogError(ex, "第 {i} 次重试，MemberName：{memberName}，FilePath：{sourceFilePath}，LineNumber：{sourceLineNumber}", i, memberName, sourceFilePath, sourceLineNumber);
        });

    public void SetProxy(Proxy? proxy)
    {
        if (proxy == null) return;
        if (proxy.ProxyType == ProxyType.Socks5)
        {
            // 域名前缀方式
            _webProxy = new WebProxy($"socks5://{proxy.Ip}:{proxy.Port}", false, null, new NetworkCredential(proxy.Username, proxy.Password));
        }
        else if (proxy.ProxyType == ProxyType.HTTP)
        {
            _webProxy = new WebProxy($"{proxy.Ip}:{proxy.Port}")
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(proxy.Username, proxy.Password)
            };
        }
    }

    public async Task ClientLoginAsync(SteamLoginState loginState)
    {
        _loginState = loginState;
        _loginState.Success = false;

        if (_loginState.ClientId != null)
        {
            if (_loginState.Requires2FA || _loginState.RequiresEmailAuth)
            {
                await UpdateAuthSessionWithSteamGuardAsync(_loginState);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(_loginState.Username) ||
                string.IsNullOrEmpty(_loginState.Password))
            {
                _loginState.Message = "请填写正确的 Steam 用户名密码";
                return;
            }

            // Steam 会从用户名和密码中删除所有非 ASCII 字符
            _loginState.Username = SteamUNPWDRegexReplace(_loginState.Username, string.Empty);
            _loginState.Password = SteamUNPWDRegexReplace(_loginState.Password, string.Empty);

            _loginState.SeesionId = _cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid");
            if (string.IsNullOrEmpty(_loginState.SeesionId))
            {
                // 访问一次登录页获取 SessionId
                await WaitAndRetryAsync().ExecuteAsync(async () =>
                {
                    await _client.GetAsync("https://store.steampowered.com/login/");
                });
                _loginState.SeesionId = _cookieContainer.GetCookieValue(new Uri(STEAM_STORE_URL), "sessionid");
            }

            var (encryptedPassword64, timestamp) = await GetRSAkeyV2Async(_loginState.Username, _loginState.Password);

            if (string.IsNullOrEmpty(encryptedPassword64))
            {
                _loginState.Message = "登录失败，获取 RSAkey 出现错误，请重试";
                _loginState.ResetStatus();
                return;
            }

            var input_protobuf_encoded = new CAuthentication_BeginAuthSessionViaCredentials_Request()
            {
                account_name = _loginState.Username,
                device_friendly_name = Default,
                encrypted_password = encryptedPassword64,
                encryption_timestamp = timestamp,
                // WebsiteId = "Community",
                platform_type = EAuthTokenPlatformType.k_EAuthTokenPlatformType_WebBrowser,
                remember_login = false
            }.ToBase64String();

            var (result, respone) = await WaitAndRetryAsync().ExecuteAsync(async () =>
            {
                var data = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "input_protobuf_encoded", input_protobuf_encoded },
                });

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/BeginAuthSessionViaCredentials/v1")
                {
                    Content = data,
                };

                request.Headers.UserAgent.Clear();
                request.Headers.UserAgent.ParseAdd(GetUserAgent());

                if (_loginState.Cookies != null)
                    _cookieContainer.Add(_loginState.Cookies);

                var respone = await _client.SendAsync(request);
                using var responeStream = await respone.Content.ReadAsStreamAsync();
                var result = Serializer.Deserialize<CAuthentication_BeginAuthSessionViaCredentials_Response>(responeStream);
                return (result, respone);
            });

            _loginState.ClientId = result.client_id;
            _loginState.SteamId = result.steamid;
            _loginState.RequestId = result.request_id;

            if (_loginState.SteamId == 0)
            {
                var eResult = respone.Headers.GetValues("X-eresult").FirstOrDefault();

                _loginState.Message = eResult switch
                {
                    "5" => "请核对您的密码和帐户名称并重试。",
                    "20" => "与 Steam 通信时出现问题。请稍后重试。",
                    "84" => "短期内来自您所在位置的失败登录过多。请15分钟后再试。",
                    _ => $"{eResult} 登录遇到未知错误，请稍后重试。",
                };

                // 短期内来自您所在位置的失败登录过多。请稍后再试。
                // 登录遇到问题，请检查账号名或密码是否正确。
                _loginState.ResetStatus();
                return;
            }

            if (result.allowed_confirmations.Any())
            {
                if (result.allowed_confirmations[0].confirmation_type == EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode)
                {
                    _loginState.Message = "需要2FA确认";
                    _loginState.Requires2FA = true;
                    _loginState.Success = false;
                    return;
                }
                else if (result.allowed_confirmations[0].confirmation_type == EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode)
                {
                    await WaitAndRetryAsync().ExecuteAsync(async () =>
                    {
                        var req = new HttpRequestMessage(HttpMethod.Post, "https://login.steampowered.com/jwt/checkdevice/" + _loginState.SteamId)
                        {
                            Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                            {
                                { "clientid", _loginState.ClientId.ToString() },
                                { "steamid", _loginState.SteamId.ToString() },
                            }),
                        };

                        req.Headers.UserAgent.Clear();
                        req.Headers.UserAgent.ParseAdd(GetUserAgent());

                        (await _client.SendAsync(req)).EnsureSuccessStatusCode();
                    });

                    _loginState.Message = "需要邮箱验证码";
                    _loginState.RequiresEmailAuth = true;
                    _loginState.Success = false;
                    return;
                }
            }
        }

        var token = await PollAuthSessionStatusAsync(_loginState.ClientId!.Value, _loginState.RequestId!);
        if (string.IsNullOrEmpty(token.RefreshToken))
        {
            _loginState.Message = "登录失败，请确认令牌是否正确。";
            _loginState.ResetStatus();
            return;
        }

        var tokens = await FinalizeLoginAsync(token.RefreshToken, _loginState.SeesionId);
        _loginState.Cookies = _cookieContainer.GetAllCookies();
        if (string.IsNullOrEmpty(tokens?.SteamId))
        {
            _loginState.Message = "FinalizeLoginAsync 登录失败";
            _loginState.ResetStatus();
            return;
        }

        _loginState.Success = true;
        _loginState.RequiresCaptcha = false;
        _loginState.Requires2FA = false;
        _loginState.RequiresEmailAuth = false;
        _loginState.Message = null;
        _loginState.AccessToken = token.AccessToken;

        if (tokens.TransferInfo?.Any() == true)
        {
            foreach (var transfer in tokens.TransferInfo)
            {
                if (transfer.Url?.Contains("help.steampowered.com") == true ||
                    transfer.Url?.Contains("steam.tv") == true)
                {
                    //跳过暂时用不到的域名 节约带宽
                    continue;
                }

                await WaitAndRetryAsync().ExecuteAsync(async () =>
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, transfer.Url)
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                              { "nonce", transfer.Params?.Nonce },
                              { "auth", transfer.Params?.Auth },
                              { "steamID", _loginState.SteamId.ToString() },
                        }),
                    };

                    req.Headers.UserAgent.Clear();
                    req.Headers.UserAgent.ParseAdd(GetUserAgent());

                    (await _client.SendAsync(req)).EnsureSuccessStatusCode();
                });
            }

            _loginState.Cookies = _cookieContainer.GetAllCookies();
        }
    }

    public async Task<HasAccountPhoneNumberResponse> HasAccountPhoneNumberAsync()
    {
        await Task.CompletedTask;
        return new HasAccountPhoneNumberResponse { HasPhoneNumber = false, PhoneNumber = "" };
    }

    public async Task<SetAccountPhoneNumberInternalResponse> SetAccountPhoneNumberAsync(string phoneNumber, string itCode)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
         {
             var data = new FormUrlEncodedContent(new Dictionary<string, string?>()
             {
                 { "access_token", _loginState.AccessToken },
                 { "phone_number", phoneNumber },
                 { "phone_country_code", itCode }
             });

             var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IPhoneService/SetAccountPhoneNumber/v1")
             {
                 Content = data
             };

             var respone = await _client.SendAsync(request);
             var result = JsonConvert.DeserializeObject<SetAccountPhoneNumberResponse>(await respone.Content.ReadAsStringAsync());
             if (result == null || string.IsNullOrWhiteSpace(result.Response?.ConfirmationEmailAddress)) throw new Exception("设置电话号码失败");
             return result;
         });

        return r?.Response;
    }

    public async Task<ConfirmAddPhoneToAccountInternalResponse> ConfirmAddPhoneToAccountAsync()
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var data = new FormUrlEncodedContent(new Dictionary<string, string?>()
            {
                 { "access_token", _loginState.AccessToken },
                 { "steamid", _loginState.SteamId.ToString() },
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IPhoneService/ConfirmAddPhoneToAccount/v1")
            {
                Content = data
            };

            var respone = await _client.SendAsync(request);
            var result = JsonConvert.DeserializeObject<ConfirmAddPhoneToAccountResponse>(await respone.Content.ReadAsStringAsync());
            return result;
        });

        return r?.Response;
    }

    public async Task<string> DoEmailConfirmationAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var respone = await _client.SendAsync(request);
        return await respone.Content.ReadAsStringAsync();
    }

    public async Task<IsAccountWaitingForEmailConfirmationInternalResponse> IsAccountWaitingForEmailConfirmationAsync()
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
         {
             var data = new FormUrlEncodedContent(new Dictionary<string, string?>()
             {
                 { "access_token", _loginState.AccessToken }
             });

             var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IPhoneService/IsAccountWaitingForEmailConfirmation/v1")
             {
                 Content = data
             };

             var respone = await _client.SendAsync(request);
             var result = JsonConvert.DeserializeObject<IsAccountWaitingForEmailConfirmationResponse>(await respone.Content.ReadAsStringAsync());
             return result;
         });

        return r?.Response;
    }

    public async Task<SendPhoneVerificationCodeInternalResponse> SendPhoneVerificationCodeAsync()
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
         {
             var data = new FormUrlEncodedContent(new Dictionary<string, string?>()
             {
                 { "access_token", _loginState.AccessToken },
                 { "language", "1" },
             });

             var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IPhoneService/SendPhoneVerificationCode/v1")
             {
                 Content = data
             };

             var respone = await _client.SendAsync(request);
             respone.EnsureSuccessStatusCode();
             var result = JsonConvert.DeserializeObject<SendPhoneVerificationCodeResponse>(await respone.Content.ReadAsStringAsync());
             return result;
         });

        return r?.Response;
    }

    public async Task<SteamGuardAccount> AddAuthenticatorAsync()
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var data = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "access_token", _loginState.AccessToken },
                { "steamid", _loginState.SteamId.ToString() },
                { "authenticator_type", "1" },
                { "device_identifier", _loginState.ClientId?.ToString() ?? string.Empty },
                { "sms_phone_id", "1" }
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/ITwoFactorService/AddAuthenticator/v1")
            {
                Content = data,
            };

            var respone = await _client.SendAsync(request);
            var result = JsonConvert.DeserializeObject<AddAuthenticatorResponse>(await respone.Content.ReadAsStringAsync());
            return result;
        });

        return r?.Response;
    }

    public async Task<VerifyAccountPhoneWithCodeInternalResponse> VerifyAccountPhoneWithCodeAsync(string code)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var data = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "access_token", _loginState.AccessToken },
                { "code", code },
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IPhoneService/VerifyAccountPhoneWithCode/v1")
            {
                Content = data,
            };

            var respone = await _client.SendAsync(request);
            respone.EnsureSuccessStatusCode();
            var result = JsonConvert.DeserializeObject<VerifyAccountPhoneWithCodeResponse>(await respone.Content.ReadAsStringAsync());
            return result;
        });

        return r?.Response;
    }

    public async Task<CTwoFactor_FinalizeAddAuthenticator_Response> FinalizeAddAuthenticatorAsync(string smsCode, string steamGuardCode)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            //var data = new FormUrlEncodedContent(new Dictionary<string, string>()
            //{
            //    { "access_token", _loginState.AccessToken },
            //    { "steamid", _loginState.SteamId.ToString() },
            //    { "activation_code", smsCode },
            //    { "authenticator_code", steamGuardCode},
            //    { "authenticator_time", Util.GetSteamTime().ToString() },
            //});

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.steampowered.com/ITwoFactorService/FinalizeAddAuthenticator/v1?access_token={_loginState.AccessToken}")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", new CTwoFactor_FinalizeAddAuthenticator_Request()
                        {
                            steamid = _loginState.SteamId,
                            activation_code = smsCode,
                            authenticator_code = steamGuardCode,
                            // TODO：使用另外一种方式实现获取时间
                            // authenticator_time = (ulong)GetSteamTime(),
                        }.ToBase64String()
                    },
                }),
            };

            var respone = await _client.SendAsync(request);
            var re = await respone.Content.ReadAsStreamAsync();
            var result = Serializer.Deserialize<CTwoFactor_FinalizeAddAuthenticator_Response>(re);
            return result;
        });

        return r;
    }

    async Task<bool> UpdateAuthSessionWithSteamGuardAsync(SteamLoginState loginState)
    {
        ArgumentNullException.ThrowIfNull(loginState.ClientId);

        var input_protobuf_encoded = new CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request()
        {
            client_id = loginState.ClientId.Value,
            steamid = loginState.SteamId,
            code = loginState.Requires2FA ? loginState.TwofactorCode : loginState.EmailCode,
            code_type = loginState.Requires2FA ? EAuthSessionGuardType.k_EAuthSessionGuardType_DeviceCode : EAuthSessionGuardType.k_EAuthSessionGuardType_EmailCode,
        }.ToBase64String();

        var result = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/UpdateAuthSessionWithSteamGuardCode/v1")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    { "input_protobuf_encoded", input_protobuf_encoded },
                }),
            };

            var respone = await _client.SendAsync(request);

            if (respone.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        });

        return result;
    }

    async Task<(string encryptedPassword64, ulong timestamp)> GetRSAkeyV2Async(string username, string password)
    {
        var data = UrlEncoder.Default.Encode(new CAuthentication_GetPasswordRSAPublicKey_Request()
        {
            account_name = username,
        }.ToBase64String());

        var requestUri = new Uri("https://api.steampowered.com/IAuthenticationService/GetPasswordRSAPublicKey/v1?input_protobuf_encoded=" + data);

        var result = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var respone = await _client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception($"获取 RSAKey 出现错误: {respone.StatusCode}");
            }

            var result = Serializer.Deserialize<CAuthentication_GetPasswordRSAPublicKey_Response>(await respone.Content.ReadAsStreamAsync());
            try
            {
                // 使用 RSA 密钥加密密码
                using var rsa = new RSACryptoServiceProvider();
                var passwordBytes = Encoding.ASCII.GetBytes(password);
                var p = rsa.ExportParameters(false);
                p.Exponent = Convert.FromHexString(result.publickey_exp);
                p.Modulus = Convert.FromHexString(result.publickey_mod);
                rsa.ImportParameters(p);
                byte[] encryptedPassword = rsa.Encrypt(passwordBytes, false);
                var encryptedPassword64 = Convert.ToBase64String(encryptedPassword);
                return (encryptedPassword64, result.timestamp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RSA 加密密码失败");
                throw;
            }
        });

        return result;
    }

    async Task<(string AccessToken, string RefreshToken)> PollAuthSessionStatusAsync(ulong client_id, byte[] request_id)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.steampowered.com/IAuthenticationService/PollAuthSessionStatus/v1")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "input_protobuf_encoded", new CAuthentication_PollAuthSessionStatus_Request()
                        {
                            client_id = client_id,
                            request_id = request_id,
                        }.ToBase64String()
                    },
                }),
            };

            var respone = await _client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception($"PollAuthSessionStatus 出现错误: {respone.StatusCode}");
            }
            var re = await respone.Content.ReadAsStreamAsync();
            var result = Serializer.Deserialize<CAuthentication_PollAuthSessionStatus_Response>(re);
            return (result.access_token, result.refresh_token);
        });
        return r;
    }

    async Task<FinalizeLoginStatus> FinalizeLoginAsync(string nonce, string? sessionid)
    {
        var r = await WaitAndRetryAsync().ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://login.steampowered.com/jwt/finalizelogin")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string?>()
                {
                    { "nonce", nonce },
                    { "sessionid", sessionid },
                    { "redir", "https://steamcommunity.com/login/home/?goto=" },
                }),
            };

            var respone = await _client.SendAsync(request);

            if (!respone.IsSuccessStatusCode)
            {
                throw new Exception($"FinalizeLoginAsync 出现错误: {respone.StatusCode}");
            }

            var reStr = await respone.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FinalizeLoginStatus>(reStr);
            if (result == null)
            {
                throw new Exception($"FinalizeLoginAsync 出现错误: {result}");
            }

            return result;
        });
        return r;
    }

    string GetUserAgent()
    {
        var userAgents = new[] { Win10EdgeLatest, Win10ChromeLatest };
        return userAgents[new Random().Next(userAgents.Length)];

    }

    string SteamUNPWDRegexReplace(string input, string replacement)
    {
        var re = new Regex("[^\\u0000-\\u007F]", RegexOptions.None, Regex.InfiniteMatchTimeout);
        return re.Replace(input, replacement);
    }

    /// <summary>
    /// Calculate the current code for the authenticator.
    /// </summary>
    /// <param name="resyncTime">flag to resync time</param>
    /// <returns>authenticator code</returns>
    public string CalculateCode(string secretKey)
    {
        char[] STEAMCHARS = new char[]
        {
            '2', '3', '4', '5', '6', '7', '8', '9', 'B', 'C',
            'D', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'Q',
            'R', 'T', 'V', 'W', 'X', 'Y',
        };

        var hmac = new HMac(new Sha1Digest());
        hmac.Init(new KeyParameter(Convert.FromBase64String(Regex.Unescape(secretKey))));

        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var codeInterval = (currentTime + _loginState.ServerTimeDiff) / (30 * 1000L);
        var codeIntervalArray = BitConverter.GetBytes(codeInterval);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(codeIntervalArray);
        hmac.BlockUpdate(codeIntervalArray, 0, codeIntervalArray.Length);

        var mac = new byte[hmac.GetMacSize()];
        hmac.DoFinal(mac, 0);

        // the last 4 bits of the mac say where the code starts (e.g. if last 4 bit are 1100, we start at byte 12)
        var start = mac[19] & 0x0f;

        // extract those 4 bytes
        var bytes = new byte[4];
        Array.Copy(mac, start, bytes, 0, 4);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        var fullcode = BitConverter.ToUInt32(bytes, 0) & 0x7fffffff;

        // build the alphanumeric code
        var code = new StringBuilder();
        for (var i = 0; i < 5; i++)
        {
            code.Append(STEAMCHARS[fullcode % STEAMCHARS.Length]);
            fullcode /= (uint)STEAMCHARS.Length;
        }

        return code.ToString();
    }

}
