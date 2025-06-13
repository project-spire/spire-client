using System.Buffers;
using System.ComponentModel;
using System.Net.Sockets;
using System.Threading.Channels;
using spire.protocol;
using spire.protocol.auth;
using spire.protocol.game;
using spire.protocol.net;

namespace spire.network;

public partial class Session : IDisposable, IAsyncDisposable
{
    public readonly Channel<byte[]> Sender = Channel.CreateUnbounded<byte[]>();

    private readonly NetworkStream _stream;
    private readonly ArrayPool<byte> _receiveBufferPool = ArrayPool<byte>.Create();
    private readonly CancellationTokenSource _cts = new();

    public Session(NetworkStream stream)
    {
        _stream = stream;
        
        _ = Receive(_cts.Token);
        _ = Send(_cts.Token);
    }
    
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        Sender.Writer.Complete();
        _stream?.Dispose();
    }
    
    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
        Sender.Writer.Complete();
        await _stream.DisposeAsync();
    }

    private async Task Receive(CancellationToken token)
    {
        var headerBuffer = new byte[ProtocolHeader.HeaderSize];
        
        while (true)
        {
            try
            {
                await _stream.ReadExactlyAsync(headerBuffer, token);

                var (category, bodyLength) = ProtocolHeader.Read(headerBuffer);
                if (category == ProtocolCategory.None)
                    throw new InvalidEnumArgumentException($"Invalid protocol category: {category}");

                var bodyBuffer = _receiveBufferPool.Rent(bodyLength);
                await _stream.ReadExactlyAsync(bodyBuffer, token);

                await Dispatch(category, bodyBuffer);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                //TODO: Log
                break;
            }
        }
    }

    private async Task Send(CancellationToken token)
    {
        while (true)
        {
            try
            {
                await foreach (var buffer in Sender.Reader.ReadAllAsync(token))
                {
                    await _stream.WriteAsync(buffer, token);
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception e)
            {
                //TODO: Log
                break;
            }
        }
    }

    private async ValueTask Dispatch(ProtocolCategory category, byte[] buffer)
    {
        try
        {
            switch (category)
            {
                case ProtocolCategory.Game:
                {
                    await Handle(GameServerProtocol.Parser.ParseFrom(buffer));
                    break;
                }
                case ProtocolCategory.Net:
                {
                    await Handle(NetServerProtocol.Parser.ParseFrom(buffer));
                    break;
                }
                case ProtocolCategory.Auth:
                {
                    await Handle(AuthServerProtocol.Parser.ParseFrom(buffer));
                    break;
                }
                case ProtocolCategory.None:
                default: break;
            }
        }
        catch (Exception e)
        {
            //TODO: Log
        }

        _receiveBufferPool.Return(buffer);
    }
}