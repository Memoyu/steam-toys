namespace SteamToys.Service.Sms.Configurations;

public class GetPhoneNumberStatus
{
    public string Code { get; set; }

    public string Status { get; set; }
}


public class GetPhoneNumberStatusResponse
{
    public string? Code { get; set; }

    public string Status { get; set; }

    public bool IsRetry { get; set; }
}
