using spire.protocol.game;

namespace spire.network;

public partial class Session
{
    private async ValueTask Handle(GameServerProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case GameServerProtocol.ProtocolOneofCase.MovementSync:
                break;
            default:
                break;
        }
    }
}