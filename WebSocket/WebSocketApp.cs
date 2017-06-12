using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace ServiceFabric.Utils.CommunicationClients.WebSocket
{
    public class WebSocketApp : IDisposable
    {
        private static readonly byte[] UncaughtHttpBytes =
            Encoding.Default.GetBytes("Uncaught error in main processing loop!");

        private string address;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;
        private HttpListener httpListener;

        public WebSocketApp(string address)
        {
            this.address = address;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {

            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }

                if (httpListener != null && httpListener.IsListening)
                {
                    httpListener.Stop();
                    httpListener.Close();
                }

                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ae)
            {
                ae.Handle(
                    ex =>
                    {
                        return true;
                    });
            }
        }

        public void Init()
        {
            if (!address.EndsWith("/"))
            {
                address += "/";
            }

            httpListener = new HttpListener();
            httpListener.Prefixes.Add(address);
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            httpListener.Start();
        }

        public async Task StartAsync(
            Func<CancellationToken, HttpListenerContext, Task<bool>> processActionAsync
            )
        {
            while (httpListener.IsListening)
            {
                HttpListenerContext context = null;
                try
                {
                    context = await httpListener.GetContextAsync();
                }
                catch (Exception ex)
                {
                    // check if the exception is caused due to cancellation
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    continue;
                }

                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // a new connection is established, dispatch to the callback function
                DispatchConnectedContext(context, processActionAsync);
            }
        }

        private void DispatchConnectedContext(
            HttpListenerContext context,
            Func<CancellationToken, HttpListenerContext, Task<bool>> processActionAsync
            )
        {
            // do not await on processAction since we don't want to block on waiting for more connections
            processActionAsync(_cancellationToken, context)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            try
                            {
                                context.Response.ContentLength64 = UncaughtHttpBytes.Length;
                                context.Response.StatusCode = 500;
                                context.Response.OutputStream.Write(UncaughtHttpBytes, 0, UncaughtHttpBytes.Length);
                                context.Response.OutputStream.Close();
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    },
                    _cancellationToken);
        }
    }
}
