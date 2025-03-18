namespace SteamToys.Contact.Model.SteamService;

public class ConfirmAddPhoneToAccountResponse
{
    [JsonProperty("response")]
    public ConfirmAddPhoneToAccountInternalResponse Response { get; set; }
}

public class ConfirmAddPhoneToAccountInternalResponse
{
    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("phone_number_type")]
    public int PhoneNumberType { get; set; }
}
