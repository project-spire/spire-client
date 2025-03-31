using spire.protocol;
using spire.protocol.net;

namespace spire.network;

public partial class Session
{
    public event Action<Pong>? PongEvent;
    
    private async ValueTask Handle(NetServerProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case NetServerProtocol.ProtocolOneofCase.Ping:
                await HandlePing(protocol.Ping);
                break;
            case NetServerProtocol.ProtocolOneofCase.Pong:
                PongEvent?.Invoke(protocol.Pong);
                break;
            default:
                break;
        }
    }
    
    private async ValueTask HandlePing(Ping ping)
    {
        var pong = new Pong
        {
            Timestamp = ping.Timestamp
        };
        var buffer = ProtocolUtil.SerializeProtocol(ProtocolCategory.Net, pong);

        await Sender.Writer.WriteAsync(buffer);
    }
}