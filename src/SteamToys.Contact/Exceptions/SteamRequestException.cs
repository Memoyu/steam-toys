namespace SteamToys.Contact.Exceptions;

public class SteamRequestException : Exception
{
    public SteamRequestException()
    {
    }

    public SteamRequestException(string message) : base(message)
    {
    }

    public SteamRequestException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
