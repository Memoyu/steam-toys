namespace SteamToys.Contact.Exceptions;

public class SmsException : Exception
{
    public SmsException()
    {
    }

    public SmsException(string message) : base(message)
    {
    }

    public SmsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
