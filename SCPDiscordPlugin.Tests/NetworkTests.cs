using SCPDiscord;
using SCPDiscord.Interface;

namespace SCPDiscordPlugin.Tests
{
    public class NetworkTests
    {
        [Fact]
        public void SendStringByID()
        {
            MemoryStream memoryStream = Utilities.MockNetworkStream();

            Utilities.ConnectNetwork();
            SCPDiscord.SCPDiscord.SendStringByID(11111111u, "TEST MESSAGE");
            Utilities.SendQueuedMessages();

            List<MessageWrapper> sentMessages = Utilities.GetSentMessages(memoryStream);

            Assert.Single(sentMessages);
            Assert.Equal(MessageWrapper.MessageOneofCase.ChatMessage, sentMessages.First().MessageCase);
            Assert.Equal("TEST MESSAGE", sentMessages.First().ChatMessage.Content);
            Assert.Equal(11111111u, sentMessages.First().ChatMessage.ChannelID);
        }
    }
}
