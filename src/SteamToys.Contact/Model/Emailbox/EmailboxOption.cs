namespace SteamToys.Contact.Model.Emailbox;

public class EmailboxOption
{
    public string Email { get; set; }

    public string Password { get; set; }

    public string Domain { get; set; }

    public int Port { get; set; }

    public bool IsSSL { get; set; }

    public EmailboxProto Proto { get; set; }
}
