using ProtoBuf;
using SteamKit2.Internal;
using System.Text.Json;
using Xunit.Abstractions;

namespace SteamToys.Service.Test
{
    public class SerializerTest
    {
        protected readonly ITestOutputHelper Output;

        public SerializerTest(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public async Task Deserialize_Test()
        {
            var b644 = "CN7nrKjuu8DiCBGaDFBbAQAQARoFTjdicDYgAg==";
            using (var st = new MemoryStream(Convert.FromBase64String(b644)))
            {
                var d = Serializer.Deserialize<CAuthentication_UpdateAuthSessionWithSteamGuardCode_Request>(st);
                Output.WriteLine(JsonSerializer.Serialize(d));
            }
        }
    }
}
