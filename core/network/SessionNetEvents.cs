using spire.protocol;
using spire.protocol.net;

namespace spire.network;

public partial class Session
{
    public event Action<Ping>? PingEvent;
    
    private void Handle(NetProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case NetProtocol.ProtocolOneofCase.Ping:
                PingEvent?.Invoke(protocol.Ping);
                HandlePing(protocol.Ping);
                break;
            default:
                break;
        }
    }
    
    private async ValueTask HandlePing(Ping ping)
    {
        var pong = new Pong
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        var buffer = ProtocolUtil.Serialize(ProtocolCategory.Net, pong);

        await Sender.Writer.WriteAsync(buffer);
    }
}