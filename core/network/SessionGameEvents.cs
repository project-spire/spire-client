using spire.protocol.game;

namespace spire.network;

public partial class Session
{
    private void Handle(GameProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case GameProtocol.ProtocolOneofCase.MovementSyncList:
                break;
            default:
                break;
        }
    }
}