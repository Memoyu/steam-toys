namespace SteamToys.Contact.Exceptions;

public class FriendlyException : Exception
{
    public FriendlyException()
    {
    }

    public FriendlyException(string message) : base(message)
    {
    }

    public FriendlyException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
