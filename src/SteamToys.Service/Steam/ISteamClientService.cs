namespace SteamToys.Service.Steam;

public interface ISteamClientService 
{
    /// <summary>
    /// 配置代理
    /// </summary>
    /// <param name="proxy"></param>
    void SetProxy(Proxy? proxy);

    /// <summary>
    /// 客户端登录
    /// </summary>
    /// <param name="loginState"></param>
    /// <returns></returns>
    Task ClientLoginAsync(SteamLoginState loginState);

    /// <summary>
    /// 校验账户是否已绑定电话号码
    /// </summary>
    /// <returns></returns>
    Task<HasAccountPhoneNumberResponse> HasAccountPhoneNumberAsync();

    /// <summary>
    /// 设置电话号码
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="itCode"></param>
    /// <returns></returns>
    Task<SetAccountPhoneNumberInternalResponse> SetAccountPhoneNumberAsync(string phoneNumber, string itCode);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <param name="itCode"></param>
    /// <returns></returns>
    Task<ConfirmAddPhoneToAccountInternalResponse> ConfirmAddPhoneToAccountAsync();

    /// <summary>
    /// 邮箱邮件确认链接点击
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<string> DoEmailConfirmationAsync(string url);

    /// <summary>
    /// 是否确认邮件
    /// </summary>
    /// <returns></returns>
    Task<IsAccountWaitingForEmailConfirmationInternalResponse> IsAccountWaitingForEmailConfirmationAsync();

    /// <summary>
    /// 发送手机验证码
    /// </summary>
    /// <returns></returns>
    Task<SendPhoneVerificationCodeInternalResponse> SendPhoneVerificationCodeAsync();

    /// <summary>
    /// 添加验证器
    /// </summary>
    /// <returns></returns>
    Task<SteamGuardAccount> AddAuthenticatorAsync();

    /// <summary>
    /// 验证手机验证码
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<VerifyAccountPhoneWithCodeInternalResponse> VerifyAccountPhoneWithCodeAsync(string code);

    /// <summary>
    /// 完成验证器添加
    /// </summary>
    /// <param name="smsCode"></param>
    /// <param name="steamGuardCode"></param>
    /// <returns></returns>
    Task<CTwoFactor_FinalizeAddAuthenticator_Response> FinalizeAddAuthenticatorAsync(string smsCode, string steamGuardCode);

    string CalculateCode(string secretKey);
}
