using spire.protocol.auth;

namespace spire.network;

public partial class Session
{
    public event Action<LoginResult>? LoginResultEvent; 
    
    private async ValueTask Handle(AuthServerProtocol protocol)
    {
        switch (protocol.ProtocolCase)
        {
            case AuthServerProtocol.ProtocolOneofCase.LoginResult:
                LoginResultEvent?.Invoke(protocol.LoginResult);
                break;
            default:
                break;
        }
    }
}