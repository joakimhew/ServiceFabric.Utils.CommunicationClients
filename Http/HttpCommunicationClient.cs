using System;
using System.Fabric;
using System.Net.Http;
using Microsoft.ServiceFabric.Services.Communication.Client;

namespace ServiceFabric.Utils.CommunicationClients.Http
{
    /// <summary>
    /// Used for IPC between services within the service fabric cluster(s)
    /// https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-connect-and-communicate-with-services
    /// </summary>
    public class HttpCommunicationClient : ICommunicationClient
    {

        /// <summary>
        /// Creates a new instance of <see cref="HttpCommunicationClient"/>
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/> to use for IPC</param>
        /// <param name="address">The address of the service to communicate with</param>
        public HttpCommunicationClient(HttpClient client, string address)
        {
            this.HttpClient = client;
            this.Url = new Uri(address);
        }

        /// <summary>
        /// Used for communication between two services over HTTP
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// The FQDN of the address to communicate with
        /// </summary>
        public Uri Url { get; }

        /// <summary>
        /// The resolved service fabric endpoint of the service based on <see cref="Url"/>
        /// </summary>
        ResolvedServiceEndpoint ICommunicationClient.Endpoint { get; set; }

        /// <summary>
        /// Name of the listener on service used in communication
        /// </summary>
        string ICommunicationClient.ListenerName { get; set; }

        /// <summary>
        /// The resolved service partition of the service based on <see cref="Url"/>
        /// </summary>
        ResolvedServicePartition ICommunicationClient.ResolvedServicePartition { get; set; }
    }
}