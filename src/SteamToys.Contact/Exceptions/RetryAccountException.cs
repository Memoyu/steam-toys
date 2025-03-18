namespace SteamToys.Contact.Exceptions;

public class RetryAccountException : Exception
{
    public RetryAccountException()
    {
    }

    public RetryAccountException(string message) : base(message)
    {
    }

    public RetryAccountException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
