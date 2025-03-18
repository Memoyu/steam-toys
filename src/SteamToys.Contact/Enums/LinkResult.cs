namespace SteamToys.Contact.Enums;

public enum LinkResult
{
    MustProvidePhoneNumber, //No phone number on the account
    MustRemovePhoneNumber, //A phone number is already on the account
    MustConfirmEmail, //User need to click link from confirmation email
    ConfirmEmailFailure,
    AwaitingFinalization, //Must provide an SMS code
    GeneralFailure, //General failure (really now!)
    AuthenticatorPresent,
    ClientAuthenticator,
}
