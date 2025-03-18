﻿global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.DependencyInjection;
global using System.Collections.Concurrent;
global using System.Runtime.CompilerServices;
global using System.Text.RegularExpressions;
global using Newtonsoft.Json;
global using Polly.Retry;
global using Polly;
global using SteamKit2.Internal;
global using ProtoBuf;
global using MailKit;
global using MailKit.Net.Imap;
global using MailKit.Net.Pop3;
global using MimeKit;
global using Mapster;

global using SteamToys.Contact.Model;
global using SteamToys.Service.MailBox;
global using SteamToys.Service.Steam;
global using SteamToys.Contact.Enums;
global using SteamToys.Contact;
global using SteamToys.Contact.Model.Emailbox;
global using SteamToys.Contact.Model.Sms;
global using SteamToys.Contact.Exceptions;
global using SteamToys.Contact.Model.Client;
global using SteamToys.Service.Sms;
global using SteamToys.Service.Sms.Configurations;
global using SteamToys.Contact.Model.SteamService;