using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SteamToys.Service.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamToys.Service.Test
{
    public class SteamClientServiceTest
    {
        private readonly ISteamClientService steamClient;

        public SteamClientServiceTest()
        {
            steamClient = new SteamClientService(NullLoggerFactory.Instance);
        }

    }
}
