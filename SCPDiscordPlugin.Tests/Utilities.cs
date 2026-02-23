using System.Reflection;
using NSubstitute;
using SCPDiscord;
using SCPDiscord.Interface;

namespace SCPDiscordPlugin.Tests;

public static class Utilities
{
  // TODO: Maybe move to a network mock class instead?
  public static MemoryStream MockNetworkStream()
  {
    SocketWrapper mockSocket = Substitute.For<SocketWrapper>();
    mockSocket.Connected.Returns(false);
    mockSocket.IsBound.Returns(false);
    mockSocket.IsConnected().Returns(false);
    mockSocket.Available.Returns(0);

    mockSocket
      .When(x => x.Connect(Arg.Any<string>(), Arg.Any<int>()))
      .Do(_ =>
      {
        mockSocket.Connected.Returns(true);
        mockSocket.IsBound.Returns(true);
        mockSocket.IsConnected().Returns(true);
      });

    mockSocket
      .When(x => x.Disconnect())
      .Do(_ =>
      {
        mockSocket.Connected.Returns(false);
        mockSocket.IsBound.Returns(false);
        mockSocket.IsConnected().Returns(false);
      });

    mockSocket
      .When(x => x.Close())
      .Do(_ =>
      {
        mockSocket.Connected.Returns(false);
        mockSocket.IsBound.Returns(false);
        mockSocket.IsConnected().Returns(false);
      });

    MemoryStream memoryStream = new();
    mockSocket.CreateNetworkStream().Returns(memoryStream);

    Network.socketWrapper = mockSocket;

    return memoryStream;
  }

  public static void ConnectNetwork()
  {
    MethodInfo? connectMethod = typeof(Network).GetMethod("Connect", BindingFlags.Static | BindingFlags.NonPublic);
    connectMethod?.Invoke(null, null);
  }

  public static void SendQueuedMessages()
  {
    MethodInfo? sendMethod = typeof(Network).GetMethod("SendQueuedMessages", BindingFlags.Static | BindingFlags.NonPublic);
    sendMethod?.Invoke(null, null);
  }

  // TODO: This class should possibly own the memory stream?
  public static List<MessageWrapper> GetSentMessages(MemoryStream stream)
  {
    List<MessageWrapper> messages = [];

    stream.Position = 0;
    while (true)
    {
      MessageWrapper message = null;
      try
      {
        message = MessageWrapper.Parser.ParseDelimitedFrom(stream);
      }
      catch(Exception) { /* ignored */ }

      if (message == null)
        break;

      messages.Add(message);
    }
    return messages;
  }
}