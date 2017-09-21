using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    public class WebSocketListener : ICommunicationListener
    {
        private const int MaxBufferSize = 102400;
        private readonly string appRoot;
        private readonly ServiceContext serviceContext;
        private readonly Func<IWebSocketConnectionHandler> createConnectionHandler;
        private readonly string serviceEndpoint;
        private string listeningAddress;
        private Task mainLoop;
        private string publishAddress;
        // Web Socket listener
        private WebSocketApp webSocketApp;

        public WebSocketListener(
            string serviceEndpoint,
            string appRoot,
            ServiceContext serviceContext,
            Func<IWebSocketConnectionHandler> createConnectionHandler
            )
        {
            this.serviceEndpoint = serviceEndpoint ?? "ServiceEndpoint";
            this.appRoot = appRoot;
            this.createConnectionHandler = createConnectionHandler;
            this.serviceContext = serviceContext;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {

            try
            {
                EndpointResourceDescription endpoint = this.serviceContext
                    .CodePackageActivationContext.GetEndpoint(this.serviceEndpoint);
                int port = endpoint.Port;

                this.listeningAddress = string.Format(
                    CultureInfo.InvariantCulture,
                    "http://+:{0}/{1}",
                    port,
                    string.IsNullOrWhiteSpace(this.appRoot)
                        ? string.Empty
                        : this.appRoot.TrimEnd('/') + '/');

                if (this.serviceContext is StatefulServiceContext)
                {
                    StatefulServiceContext sip = (StatefulServiceContext)this.serviceContext;
                    this.listeningAddress += sip.PartitionId + "/" + sip.ReplicaId + "/";
                }

                this.publishAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

                this.publishAddress = this.publishAddress.Replace("http", "ws");

                this.webSocketApp = new WebSocketApp(this.listeningAddress);
                this.webSocketApp.Init();

                this.mainLoop = this.webSocketApp.StartAsync(this.ProcessConnectionAsync);

                return await Task.FromResult(this.publishAddress);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopAll();
            return Task.FromResult(true);
        }

        public void Abort()
        {
            this.StopAll();
        }

        /// <summary>
        ///     Stops, cancels, and disposes everything.
        /// </summary>
        private void StopAll()
        {

            try
            {
                this.webSocketApp.Dispose();
                if (this.mainLoop != null)
                {
                    // allow a few seconds to complete the main loop
                    if (!this.mainLoop.Wait(TimeSpan.FromSeconds(3)))
                    {
                    }

                    this.mainLoop.Dispose();
                    this.mainLoop = null;
                }

                this.listeningAddress = string.Empty;
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private async Task<bool> ProcessConnectionAsync(
            CancellationToken cancellationToken,
            HttpListenerContext httpContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
            }
            catch (Exception ex)
            {
                // The upgrade process failed somehow. For simplicity lets assume it was a failure on the part of the server and indicate this using 500.
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                return false;
            }

            System.Net.WebSockets.WebSocket webSocket = webSocketContext.WebSocket;
            MemoryStream ms = new MemoryStream();
            try
            {
                IWebSocketConnectionHandler handler = this.createConnectionHandler();

                byte[] receiveBuffer = null;

                // While the WebSocket connection remains open run a simple loop that receives data and sends it back.
                while (webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        if (receiveBuffer == null)
                        {
                            receiveBuffer = new byte[MaxBufferSize];
                        }

                        WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken);
                        if (receiveResult.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                            continue;
                        }

                        if (receiveResult.EndOfMessage)
                        {
                            await ms.WriteAsync(receiveBuffer, 0, receiveResult.Count, cancellationToken);
                            receiveBuffer = ms.ToArray();
                            ms.Dispose();
                            ms = new MemoryStream();
                        }
                        else
                        {
                            await ms.WriteAsync(receiveBuffer, 0, receiveResult.Count, cancellationToken);
                            continue;
                        }

                        byte[] wsresponse = null;
                        try
                        {
                            // dispatch to App provided function with requested payload
                            wsresponse = await handler.ProcessWebSocketMessageAsync(receiveBuffer, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            // catch any error in the appAction and notify the client

                        }

                        // Send Result back to client
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(wsresponse),
                            WebSocketMessageType.Text,
                            true,
                            cancellationToken);
                    }
                    catch (WebSocketException ex)
                    {
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
