﻿namespace SteamToys.Contact.Model;

public class Proxy
{
    public string Ip { get; set; }

    public int Port { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public ProxyType ProxyType { get; set; }
}
