namespace SteamToys.Service.ServiceCollectionExtensions;

public static class SmsServiceCollectionExtensions
{
    public static IServiceCollection AddSms(this IServiceCollection services)
    {
        services.AddSingleton<ISmsProvider, OnlineSimProvider>();
        services.AddSingleton<ISmsProvider, FiveSimProvider>();
        services.AddSingleton<ISmsProvider, SmsActivateProvider>();

        services.AddSingleton<ISmsService, SmsService>();
        return services;
    }
}
