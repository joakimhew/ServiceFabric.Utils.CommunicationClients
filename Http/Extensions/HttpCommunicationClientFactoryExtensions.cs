using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using ServiceFabric.Utils.CommunicationClients.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceFabric.Utils.Shared.Extensions
{
    /// <summary>
    /// Provides extension methods for calling another service asynchronously.
    /// </summary>
    public static class HttpCommunicationClientFactoryExtensions
    {
        /// <summary> 
        /// Send a GET request to the specified Uri as an asynchronous 
        /// operation. 
        /// </summary> 
        /// <param name="communicationClientFactory">THe communication client factory</param> 
        /// <param name="serviceUri">The service Uri the request is sent to</param> 
        /// <param name="url">The Url the request is sent to</param> 
        /// <param name="authorization">Authentication header</param> 
        /// <returns> 
        /// Returns <see cref="Task{TResult}"/>. The task object representing the asynchronous 
        /// operation. 
        /// </returns> 
        public static async Task<HttpResponseMessage> ServiceGetAsync(this HttpCommunicationClientFactory communicationClientFactory, Uri serviceUri, string url, AuthenticationHeaderValue authorization = null)
        {
            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                communicationClientFactory,
                serviceUri,
                ServicePartitionKey.Singleton);

            return await partitionClient.InvokeWithRetryAsync(
                async (client) =>
                {
                    if (null != authorization)
                    {
                        client.HttpClient.DefaultRequestHeaders.Authorization = authorization;
                    }

                    return await client.HttpClient.GetAsync(new Uri(client.Url, url), CancellationToken.None);
                });
        }

        /// <summary> 
        /// Sends a POST request as an asynchronous operation to the specified Uri with the 
        /// given value serialized as JSON. 
        /// </summary> 
        /// <typeparam name="T"></typeparam> 
        /// <param name="communicationClientFactory">THe communication client factory</param> 
        /// <param name="serviceUri">The service Uri the request is sent to</param> 
        /// <param name="url">The Url the request is sent to</param> 
        /// <param name="value">The value to be sent in the request</param> 
        /// <param name="authorization">Authentication header</param> 
        /// <returns> 
        /// Returns <see cref="Task{TResult}"/>. The task object representing the asynchronous 
        /// operation. 
        /// </returns> 
        public static async Task<HttpResponseMessage> ServicePostAsync<T>(this HttpCommunicationClientFactory communicationClientFactory, Uri serviceUri, string url, T value, AuthenticationHeaderValue authorization = null)
        {

            var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(
                communicationClientFactory,
                serviceUri,
                ServicePartitionKey.Singleton);

            return await partitionClient.InvokeWithRetryAsync(
                async client =>
                {
                    if (null != authorization)
                    {
                        client.HttpClient.DefaultRequestHeaders.Authorization = authorization;
                    }

                    return await client.HttpClient.PostAsJsonAsync(new Uri(client.Url, url), value);
                });
        }
    }
} 

