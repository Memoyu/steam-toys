namespace SteamToys.Service.Sms;

public interface ISmsService
{
    Task<GetPhoneNumber> GetPhoneNumberAsync(SmsPlatform platform, string service, string country);

    Task<GetPhoneNumberStatus> GetPhoneNumberStatusAsync(GetPhoneNumberStatusRequest request);

    Task DestroyPhoneNumberAsync(DestroyPhoneNumberItem destroy);
}
