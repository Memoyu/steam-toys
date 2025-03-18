namespace SteamToys.Service.MailBox;

public interface IMailboxService
{
    /// <summary>
    /// 获取邮箱令牌
    /// </summary>
    /// <param name="option"></param>
    /// <param name="waitTime"></param>
    /// <param name="bindTime"></param>
    /// <returns></returns>
    Task<string> GetLoginCodeRamblerAsync(EmailboxOption option, int waitTime , DateTime bindTime);

    /// <summary>
    /// 邮箱确认手机号绑定
    /// </summary>
    /// <param name="option"></param>
    /// <param name="waitTime"></param>
    /// <param name="bindTime"></param>
    /// <param name="toSteamConfirmPhoneFunc"></param>
    /// <returns></returns>
    Task<bool> ReadEmailAndConfirmationAsync(EmailboxOption option, int waitTime, DateTime bindTime, Func<string, Task<string>> toSteamConfirmPhoneFunc);
}
