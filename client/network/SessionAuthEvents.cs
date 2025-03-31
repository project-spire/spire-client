using spire.protocol.auth;

namespace spire.network;

public partial class Session
{
    private async ValueTask Handle(AuthServerProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case AuthServerProtocol.ProtocolOneofCase.LoginResult:
                break;
            default:
                break;
        }
    }
}