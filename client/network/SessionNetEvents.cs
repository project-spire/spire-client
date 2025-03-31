using spire.protocol;
using spire.protocol.net;

namespace spire.network;

public partial class Session
{
    public event Action<Ping>? PingEvent;
    
    private async ValueTask Handle(NetServerProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case NetServerProtocol.ProtocolOneofCase.Ping:
                PingEvent?.Invoke(protocol.Ping);
                await HandlePing(protocol.Ping);
                break;
            default:
                break;
        }
    }
    
    private async ValueTask HandlePing(Ping ping)
    {
        // Pong
        var buffer = ProtocolUtil.SerializeProtocol(ProtocolCategory.Net, ping);

        await Sender.Writer.WriteAsync(buffer);
    }
}