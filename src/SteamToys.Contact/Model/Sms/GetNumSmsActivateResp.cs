namespace SteamToys.Contact.Model.Sms;

public class GetNumSmsActivateResp
{
    public long ActivationId { get; set; }

    public string PhoneNumber { get; set; }

    public string ActivationCost { get; set; }

    public string CountryCode { get; set; }

    public string CanGetAnotherSms { get; set; }

    public string ActivationTime { get; set; }

    public string ActivationOperator { get; set; }
}
