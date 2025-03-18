using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using SteamToys.Contact.Model;
using SteamToys.Service.Sms;

namespace SteamToys.Service.Test.Sms;

public class FiveSimProviderTest
{
    private readonly ISmsProvider smsProvider;

    public FiveSimProviderTest()
    {
        IServiceCollection services = new ServiceCollection();
        IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        services.AddHttpClient("5-sim", c =>
        {
            c.BaseAddress = new Uri("https://5sim.net/");
        });
        services.Configure<AppSetting>(builder.Build());
        services.AddSingleton<ISmsProvider, FiveSimProvider>();
        
        var serviceProvider = services.BuildServiceProvider();
        smsProvider = serviceProvider.GetRequiredService<ISmsProvider>();
    }

    [Fact]
    public async Task Destroy_Phone_Number_Test()
    {
        var res = await smsProvider.GetPhoneNumberAsync("steam", "england");
        Assert.NotNull(res);
        Assert.NotEmpty(res.PhoneNumber);
        await Task.Delay(60 * 1000);
        var d = await smsProvider.DestroyPhoneNumberAsync(res.Id);

        Assert.True(d);
    }
}
