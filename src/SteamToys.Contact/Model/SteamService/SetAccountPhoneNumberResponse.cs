namespace SteamToys.Contact.Model.SteamService;

public class SetAccountPhoneNumberResponse
{
    [JsonProperty("response")]
    public SetAccountPhoneNumberInternalResponse Response { get; set; }
}

public class SetAccountPhoneNumberInternalResponse
{

    [JsonProperty("confirmation_email_address")]
    public string ConfirmationEmailAddress { get; set; }

    [JsonProperty("phone_number_formatted")]
    public string PhoneNumberFormatted { get; set; }

}
