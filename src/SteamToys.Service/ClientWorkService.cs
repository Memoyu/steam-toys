using SteamToys.Contact.Const;
using SteamToys.Contact.Enums;
using SteamToys.Contact.Model;
using SteamToys.Service.Sms;

namespace SteamToys.Service;

public class ClientWorkService : IWorkService
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMailboxService _mailboxService;
    private readonly ISmsService _smsService;

    private ISteamClientService _steamClientService;
    private ConcurrentQueue<SteamAccount> Queues = new ConcurrentQueue<SteamAccount>();

    public ClientWorkService(
        ILoggerFactory loggerFactory,
        IServiceProvider serviceProvider,
        ISmsService smsService,
        IMailboxService mailboxService)
    {
        _logger = loggerFactory.CreateLogger<ClientWorkService>();
        _serviceProvider = serviceProvider;
        _smsService = smsService;
        _mailboxService = mailboxService;
    }

    public async Task DoWorkWithRetryAsync(AppSetting config, List<SteamAccount> accounts, CancellationToken token)
    {
        accounts.ForEach(m => Queues.Enqueue(m));
        var requestRetry = config.RequestRetry;
        var accountRetry = config.AccountRetry;
        var waitCode = config.WaitCodeTime;
        var isGetTradeoffers = config.IsGetTradeoffers;
        var privacy = config.Privacy;
        var outputPath = config.InitOutputPath();
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

        // 跑流程
        List<Task> tasks = new List<Task>();
        var thread = accounts.Count < config.Thread ? accounts.Count : config.Thread;
        for (int i = 0; i < thread; i++)
        {
            if (token.IsCancellationRequested)
            {
                // 在取消请求时清理资源或执行其他操作
                break;
            }

            tasks.Add(Task.Factory.StartNew(async () =>
            {
                // var g = Guid.NewGuid().ToString();
                // _logger.LogInformation($"Thread Begin: {g}");
                while (Queues.Count > 0)
                {
                    if (token.IsCancellationRequested)
                        break;

                    var res = Queues.TryDequeue(out var accountItem);
                    if (res)
                    {
                        // 随机获取代理
                        Proxy? proxy = null;
                        if (proxies != null && proxies.Any())
                        {
                            Random rd = new Random();
                            var s = rd.Next(0, proxies.Count);
                            proxy = proxies[s];
                        }

                        var account = accounts.FirstOrDefault(m => m.Id == accountItem?.Id);
                        if (account == null) continue;
                        await DoWorkAsync(new UserLogin
                        {
                            Account = account,
                            OutputPath = outputPath,
                            Privacy = privacy,
                            EmailboxOption = new EmailboxOption
                            {
                                Email = account.Email,
                                Password = account.EmailPassword,
                                Domain = isCustomDomain ? customDomain : Util.GetMailboxHost(account.Email, emailboxPrefix),
                                Port = emailboxPort,
                                IsSSL = emailboxIsSSL,
                                Proto = emailboxProtocol,
                            },
                            SmsOption = new SmsOption
                            {
                                Platform = smsPlatform,
                                Service = smsService.Service,
                                Country = smsCountry,
                            },
                            OtherOption = new OtherOption
                            {
                                Proxy = proxy,
                                RequestRetry = requestRetry,
                                AccountRetry = accountRetry,
                                WaitCodeTime = waitCode,
                                IsGetTradeoffers = isGetTradeoffers
                            }
                        });
                    }

                    await Task.Delay(500);
                }
                // _logger.LogInformation($"Thread End: {g}");

            }, TaskCreationOptions.LongRunning).Unwrap());

            // 延时队列延时，错开线程
            await Task.Delay(2000);
        }

        await Task.WhenAll(tasks.ToArray());
    }

    private async Task DoWorkAsync(UserLogin login)
    {
        var beginBindTime = DateTime.Now;
        var account = login.Account;
        var steam = account.Steam;
        var steamPassword = account.SteamPassword;
        var loginState = new SteamLoginState
        {
            Username = steam,
            Password = steamPassword,
        };

        // 配置代理信息及重试次数(因为WPF 页面不会重载，所以DI没有Transient，需要每次都构造新的)
        _steamClientService = _serviceProvider.GetRequiredService<ISteamClientService>();
        _steamClientService.SetProxy(login.OtherOption.Proxy);

        // 初始化数据
        account.Init();

        account.SmsPlatform = login.SmsOption.Platform.GetDescription();
        GetPhoneNumber smsInfo = null;

        _logger.LogInformation($"{steam}: ..........手机令牌绑定开始.........");
        _logger.LogInformation($"{steam}: 代理：{JsonConvert.SerializeObject(login.OtherOption.Proxy)}");
        _logger.LogInformation($"{steam}：登录执行.........");
        account.Info("进行登录");

        try
        {
            var verifyRes = login.VerifyReq();
            if (!verifyRes.Success)
            {
                _logger.LogInformation($"{steam}：{verifyRes.Msg}");
                account.Fail(verifyRes.Msg);
                return;
            }

        // Client登录
        RetryLogin:
            await _steamClientService.ClientLoginAsync(loginState);

            if (!loginState.Success)
            {
                if (loginState.RequiresEmailAuth)
                {
                    var code = await _mailboxService.GetLoginCodeRamblerAsync(login.EmailboxOption, login.OtherOption.WaitCodeTime, beginBindTime);
                    if (string.IsNullOrWhiteSpace(code))
                    {
                        account.Fail($"登录失败：获取邮箱验证码失败");
                        return;
                    }

                    _logger.LogInformation($"{steam}：登录验证码: {code}");
                    loginState.EmailCode = code;
                    if (account.RetryLoginCount < login.OtherOption.AccountRetry)
                    {
                        account.RetryLoginCount++;
                        goto RetryLogin;
                    }
                }

                account.Fail($"登录失败：{loginState.Message}");
                return;
            }

            // TODO：判断是否绑定了电话号码
            await _steamClientService.HasAccountPhoneNumberAsync();

        }
        catch (Exception ex)
        {
            account.Fail($"登录失败：{loginState.Message}");
            _logger.LogError(ex, $"{steam}：登录异常 msg:{ex.Message}");
            return;
        }


    RetryBind:
        try
        {
            account.Info("绑定手机号码");
            _logger.LogInformation($"{steam}：绑定手机号码");
            // 获取电话号码
            var itCode = login.SmsOption.Country.ITCode;
            var smsResp = await _smsService.GetPhoneNumberAsync(login.SmsOption.Platform, login.SmsOption.Service, login.SmsOption.Country.Country);
            if (string.IsNullOrWhiteSpace(smsResp.PhoneNumber))
            {
                account.Fail("获取手机号码失败");
                _logger.LogError($"{steam}：获取手机号码失败");
                throw new RetryAccountException();
            }
            smsInfo = smsResp;
            var formatedPhoneNumber = Util.FilterPhoneNumber(smsResp.PhoneNumber, itCode);
            account.PhoneNumber = formatedPhoneNumber;

            // 绑定电话号码
            var setPhoneNumberResp = await _steamClientService.SetAccountPhoneNumberAsync(formatedPhoneNumber, itCode);
            _logger.LogInformation($"{steam}：设置手机号码：{JsonConvert.SerializeObject(setPhoneNumberResp)}");
            if (string.IsNullOrWhiteSpace(setPhoneNumberResp.ConfirmationEmailAddress))
            {
                _logger.LogError($"{steam}：设置手机号码失败");
                account.Fail("设置手机号码失败");
                throw new RetryAccountException();
            }

            account.Info("邮件确认绑定");
            _logger.LogInformation($"{steam}：邮件确认绑定");
            // 邮箱邮件确认手机号码绑定
            var emailConfirmationResp = await _mailboxService.ReadEmailAndConfirmationAsync(login.EmailboxOption, login.OtherOption.WaitCodeTime, beginBindTime, _steamClientService.DoEmailConfirmationAsync);
            if (!emailConfirmationResp)
            {
                _logger.LogError($"{steam}：邮件确认绑定失败");
                account.Fail("邮件确认绑定失败");
                throw new RetryAccountException();
            }

            // 确定邮件状态
            account.Info("确认邮件状态");
            var isEmailConfirmation = await _steamClientService.IsAccountWaitingForEmailConfirmationAsync();
            _logger.LogInformation($"{steam}：确认邮件状态：{JsonConvert.SerializeObject(isEmailConfirmation)}");
            if (isEmailConfirmation.AwaitingEmailConfirmation)
            {
                _logger.LogError($"{steam}：确认邮件状态 - 未完成");
                account.Fail("确认邮件状态失败");
                throw new RetryAccountException();
            }

            // 发送手机验证码
            account.Info("发送验手机证码");
            await Task.Delay(1000); // 加个延时试试，总感觉短信发不出去
            var sendVerificationCodeResp = await _steamClientService.SendPhoneVerificationCodeAsync();
            _logger.LogInformation($"{steam}：手机验证码已发出");

            // 添加验证器
            account.Info($"{steam}：添加验证器");
            var retryAddAuthenticator = 0;

        RetryAddAuthenticator:
            var addAuthenticatorResp = await _steamClientService.AddAuthenticatorAsync();

            if (addAuthenticatorResp == null)
            {
                _logger.LogError($"{steam}：添加验证器失败");
                account.Fail("添加验证器失败");
                throw new RetryAccountException();
            }
            _logger.LogInformation($"{steam}：添加验证器: {JsonConvert.SerializeObject(addAuthenticatorResp)}");

            // 可能存在数据延迟，Status == 2时，需要做几次重试
            if (addAuthenticatorResp.Status == 2 && retryAddAuthenticator < 3)
            {
                retryAddAuthenticator++;
                await Task.Delay(2000);
                goto RetryAddAuthenticator;
            }


            if (addAuthenticatorResp.Status == 29)
            {
                _logger.LogError($"{steam}：验证器已存在");
                account.Fail("验证器已存在");
                throw new RetryAccountException();
            }

            if (addAuthenticatorResp.Status != 1)
            {
                _logger.LogError($"{steam}：添加验证器失败 Status：{addAuthenticatorResp.Status}");
                account.Fail($"添加验证器失败");
                throw new RetryAccountException();
            }
            account.RecoverCode = addAuthenticatorResp.RevocationCode;

            // 获取手机验证码
            account.Info("获取手机验证码");
            var smsStatusRequest = new GetPhoneNumberStatusRequest
            {
                Platform = login.SmsOption.Platform,
                Id = smsResp.Id,
                WaitTime = login.OtherOption.WaitCodeTime
            };
            var smsStatusResp = await _smsService.GetPhoneNumberStatusAsync(smsStatusRequest);
            _logger.LogInformation($"{steam}：获取手机验证码: {JsonConvert.SerializeObject(smsResp)}");
            if (string.IsNullOrEmpty(smsStatusResp.Code))
            {
                _logger.LogError($"{steam}：未收到手机验证码");
                account.Fail("未收到手机验证码");
                account.BindError = BindError.BadSmsCode;
                throw new RetryAccountException();
            }
            account.Captcha = smsStatusResp.Code;

            // 验证手机验证码
            account.Info("验证手机验证码");
            var verifyAccountPhoneResp = await _steamClientService.VerifyAccountPhoneWithCodeAsync(smsStatusResp.Code);
            _logger.LogInformation($"{steam}：验证手机验证码: {JsonConvert.SerializeObject(verifyAccountPhoneResp)}");

            // 完成验证器添加
            var finalizeEnum = FinalizeResult.GeneralFailure;
            int tries = 0;
            while (tries <= 30)
            {
                account.Info("完成验证器添加");
                var finalizeResp = await _steamClientService.FinalizeAddAuthenticatorAsync(smsStatusResp.Code, addAuthenticatorResp.GenerateSteamGuardCode());
                _logger.LogInformation($"{steam}：完成验证器添加: {JsonConvert.SerializeObject(finalizeResp)}");
                if (finalizeResp == null)
                {
                    finalizeEnum = FinalizeResult.GeneralFailure;
                    break;
                }

                if (finalizeResp.status == 89)
                {
                    finalizeEnum = FinalizeResult.BadSMSCode;
                    break;
                }

                if (finalizeResp.status == 88)
                {
                    if (tries >= 30)
                    {
                        finalizeEnum = FinalizeResult.UnableToGenerateCorrectCodes;
                        break;
                    }
                }

                if (!finalizeResp.success)
                {
                    finalizeEnum = FinalizeResult.GeneralFailure;
                    break;
                }

                if (finalizeResp.want_more)
                {
                    tries++;
                    continue;
                }

                finalizeEnum = FinalizeResult.Success;
                break;
            }

            switch (finalizeEnum)
            {
                case FinalizeResult.UnableToGenerateCorrectCodes:
                    _logger.LogError($"{steam}：无法生成正确的代码来完成此身份验证器，恢复码：{addAuthenticatorResp.RevocationCode}");
                    account.Fail("完成验证器添加失败");
                    throw new RetryAccountException();

                case FinalizeResult.BadSMSCode:
                    _logger.LogError($"{steam}：手机验证码错误");
                    account.Fail("手机验证码错误");
                    throw new RetryAccountException();

                case FinalizeResult.GeneralFailure:
                    _logger.LogError($"{steam}：完成验证器添加失败");
                    account.Fail("完成验证器添加失败");
                    throw new RetryAccountException();
            }

            try
            {
                addAuthenticatorResp.Session = new SessionData { SteamID = loginState.SteamId };
                await Util.SaveToMaFileAsync(addAuthenticatorResp, loginState.SteamId, login.OutputPath);
                await Util.SaveToSucceedLogFileAsync(steam);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{steam}：写入maFile异常");
            }

            account.Info($"手机令牌绑定成功！");
            account.BindStatus = "成功";
        }
        catch (Exception ex)
        {
            // account.Fail(ex.Message);
            _logger.LogError(ex, $"{steam}：{ex.Message}");

            // 尝试重试
            _logger.LogInformation($"{steam}：进行绑定手机号重试");
            if (account.RetryBindCount < login.OtherOption.AccountRetry)
            {
                account.RetryBindCount++;
                await ToDestroyPhoneNumberAsync(smsInfo);
                goto RetryBind;
            }
        }
        finally
        {
            await ToDestroyPhoneNumberAsync(smsInfo);
        }

        account.BindDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _logger.LogInformation($"{steam}: ..........手机令牌绑定结束.........");
    }

    private async Task ToDestroyPhoneNumberAsync(GetPhoneNumber smsInfo)
    {
        // 已获取电话号码，就需要进行销毁
        if (smsInfo != null && !string.IsNullOrWhiteSpace(smsInfo.Id))
        {
            _logger.LogInformation($"开始进行销毁：{JsonConvert.SerializeObject(smsInfo)}");
            await _smsService.DestroyPhoneNumberAsync(new DestroyPhoneNumberItem
            {
                Platform = smsInfo.Platform,
                Id = smsInfo.Id,
                GenerateTime = smsInfo.GenerateTime,
                PhoneNumber = smsInfo.PhoneNumber,
            });
        }
    }
}
