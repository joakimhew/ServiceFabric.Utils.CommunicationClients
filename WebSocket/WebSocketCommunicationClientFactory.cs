using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    public class WebSocketCommunicationClientFactory : CommunicationClientFactoryBase<WebSocketCommunicationClient>
    {
        //TODO: Create exceptionhandler to pass into base constructor
        public WebSocketCommunicationClientFactory(IServicePartitionResolver resolver = null, IEnumerable<IExceptionHandler> exceptionHandlers = null)
            :base(resolver, null)
        {

        }

        private static readonly TimeSpan MaxRetryBackoffIntervalOnNonTransientErrors = TimeSpan.FromSeconds(3);

        protected override bool ValidateClient(WebSocketCommunicationClient client)
        {
            return client.ValidateClient();
        }

        protected override bool ValidateClient(string endpoint, WebSocketCommunicationClient client)
        {
            return client.ValidateClient(endpoint);
        }

        protected override async Task<WebSocketCommunicationClient> CreateClientAsync(
            string endpoint,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(endpoint) || !endpoint.StartsWith("ws"))
            {
                throw new InvalidOperationException("The endpoint address is not valid. Please resolve again.");
            }

            string endpointAddress = endpoint;

            if (!endpointAddress.EndsWith("/"))
            {
                endpointAddress = endpointAddress + "/";
            }

            WebSocketCommunicationClient client = new WebSocketCommunicationClient(endpointAddress);
            await client.ConnectAsync(cancellationToken);

            return client;
        }

        protected override void AbortClient(WebSocketCommunicationClient client)
        {
            // Http communication doesn't maintain a communication channel, so nothing to abort.

        }
    }
}
