using Microsoft.Extensions.Logging.Abstractions;
using SteamToys.Service.MailBox;

namespace SteamToys.Service.Test.Mailbox
{
    public class MailboxServiceTest
    {
        private readonly IMailboxService mailboxService;

        public MailboxServiceTest()
        {
            mailboxService = new MailboxService(NullLoggerFactory.Instance);
        }

        [Fact]
        public async Task Get_Steam_Guard_Code_Test()
        {
            var code = await mailboxService.GetLoginCodeRamblerAsync(
                new Contact.Model.Emailbox.EmailboxOption
                {
                    Email = "xf101259@fallinin.club",
                    Password = "ML18556773",
                    Domain = "mail.fallinin.club",
                    Port = 993,
                    Proto = Contact.Enums.EmailboxProto.IMAP,
                    IsSSL = true
                }, 2, DateTime.Now);

            Assert.NotEmpty(code);
        }

    }
}
