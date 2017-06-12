using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    /// <summary>
    /// Used for handling websocket IPC
    /// </summary>
    public interface IWebSocketConnectionHandler
    {
        Task<byte[]> ProcessWebSocketMessageAsync(byte[] webSocketRequest, CancellationToken cancellationToken);
    }
}
