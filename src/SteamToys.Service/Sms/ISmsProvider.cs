namespace SteamToys.Service.Sms;

public interface ISmsProvider
{
    SmsPlatform Platform { get; }

    Task<GetPhoneNumberResponse> GetPhoneNumberAsync(string service, string country);

    Task<GetPhoneNumberStatusResponse> GetPhoneNumberStatusAsync(string id);

    Task<bool> DestroyPhoneNumberAsync(string id);
}
