using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.CommunicationClients.Http
{
    /// <summary>
    /// Factory used to create <see cref="HttpCommunicationClient"/>
    /// </summary>
    public class HttpCommunicationClientFactory : CommunicationClientFactoryBase<HttpCommunicationClient>
    {

        private readonly HttpClient _httpClient = new HttpClient();

        /// <summary>
        /// Creates a new instance of <see cref="HttpCommunicationClientFactory"/>
        /// </summary>
        /// <param name="resolver">Used to resolve endpoints and partitions of the service that we wish to communicate with</param>
        /// <param name="exceptionHandlers">The exception handlers to used while communicating</param>
        public HttpCommunicationClientFactory(IServicePartitionResolver resolver = null, IEnumerable<IExceptionHandler> exceptionHandlers = null)
            : base(resolver, CreateExceptionHandlers(exceptionHandlers))
        {
        }

        /// <summary>
        /// Validates the client. This is not needed for HTTP and is set to true
        /// </summary>
        /// <param name="client">The <see cref="HttpCommunicationClient"/> to validate</param>
        /// <returns>true</returns>
        protected override bool ValidateClient(HttpCommunicationClient client)
        {
            return true;
        }

        /// <summary>
        /// Validates the client. This is not needed for HTTP and is set to true
        /// </summary>
        /// <param name="endpoint">The endpoint that to validate against</param>
        /// <param name="client">The <see cref="HttpCommunicationClient"/> to validate</param>
        /// <returns>true</returns>
        protected override bool ValidateClient(string endpoint, HttpCommunicationClient client)
        {
            return true;
        }

        /// <summary>
        /// Creates a new <see cref="HttpCommunicationClient"/>
        /// </summary>
        /// <param name="endpoint">The service fabric endpoint of the service</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="HttpCommunicationClient"/></returns>
        protected override Task<HttpCommunicationClient> CreateClientAsync(string endpoint, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpCommunicationClient(_httpClient, endpoint));
        }

        /// <summary>
        /// Not used for HTTP.
        /// </summary>
        /// <param name="client"></param>
        protected override void AbortClient(HttpCommunicationClient client)
        {
        }

        /// <summary>
        /// Creates the exception handlers with optional additional exception handlers <see cref="IExceptionHandler"/>
        /// </summary>
        /// <param name="additionalExceptionHandlers">Additional exception handlers if any</param>
        /// <returns></returns>
        private static IEnumerable<IExceptionHandler> CreateExceptionHandlers(
            IEnumerable<IExceptionHandler> additionalExceptionHandlers)
        {
            return null;
        }



        /// <summary> 
        /// Creates a new instance of <see cref="HttpCommunicationClientFactory"/> with default <see cref="ServicePartitionResolver"/>. 
        /// </summary> 
        /// <returns></returns> 
        public static HttpCommunicationClientFactory CreateDefault()
        {
            return new HttpCommunicationClientFactory(new ServicePartitionResolver(() => new System.Fabric.FabricClient()));
        }
    }
}
