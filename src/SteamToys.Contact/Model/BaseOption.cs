namespace SteamToys.Contact.Model;

public class BaseOption : BaseOption<int>
{

}

public class BaseOption<T>
{
    public T Id { get; set; }

    public string Name { get; set; }

}
