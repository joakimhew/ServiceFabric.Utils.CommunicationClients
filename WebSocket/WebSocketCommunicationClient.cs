using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Fabric;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    public class WebSocketCommunicationClient : ICommunicationClient
    {
        private ClientWebSocket _clientWebSocket = null;

        public WebSocketCommunicationClient(string baseAddress)
        {
            _clientWebSocket = new ClientWebSocket();

            BaseAddress = baseAddress;
        }

        /// <summary>
        /// Base address of the client
        /// </summary>
        public string BaseAddress { get; }

        /// <summary>
        /// The resolved service partition which contains the resolved service endpoints.
        /// </summary>
        public ResolvedServicePartition ResolvedServicePartition { get; set; }

        public string ListenerName { get; set; }

        public ResolvedServiceEndpoint Endpoint { get; set; }

        public async Task<byte[]> SendReceiveAsync(byte[] payload)
        {
            byte[] receiveBytes = new byte[10240];

            // Send request operation
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);

            WebSocketReceiveResult receiveResult =
                await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBytes), CancellationToken.None);

            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(receiveBytes, 0, receiveResult.Count);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal bool ValidateClient()
        {
            if (_clientWebSocket == null)
            {
                return false;
            }

            if (_clientWebSocket.State != WebSocketState.Open && _clientWebSocket.State != WebSocketState.Connecting)
            {
                _clientWebSocket.Dispose();
                _clientWebSocket = null;
                return false;
            }

            return true;
        }

        internal bool ValidateClient(string endpoint)
        {
            if (this.BaseAddress == endpoint)
            {
                return true;
            }

            _clientWebSocket.Dispose();
            _clientWebSocket = null;
            return false;
        }

        internal async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await _clientWebSocket.ConnectAsync(new Uri(this.BaseAddress), cancellationToken);
        }
    }
}
