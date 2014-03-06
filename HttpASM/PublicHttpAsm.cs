using System;
using System.Net;
using System.Text.RegularExpressions;
using Common;
using WebASM;

namespace HttpASM
{
    public class PublicHttpAsm
    {
        private readonly PublicWebAsm _actualPublicAsm;
        private readonly HttpListener _listener;
        private static readonly Regex RegEx5M = new Regex("/Class/([^/]+)/Token$");

        public PublicHttpAsm(PublicWebAsm actualPublicAsm, string address)
        {
            _actualPublicAsm = actualPublicAsm;
            _listener = new HttpListener();
            _listener.Prefixes.Add(address);
        }

        public void Start()
        {
            _listener.Start();
            _listener.BeginGetContext(ListenerCallback, _listener);
        }

        public void Stop()
        {
            _listener.Stop();
        }

        public PublicHttpAsm(IServe5M actual5M, string address)
            : this(new PublicWebAsm(actual5M), address)
        {
        }

        private  void ListenerCallback(IAsyncResult result)
        {
            var listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            var context = listener.EndGetContext(result);
            context.Response.KeepAlive = true;
            _listener.BeginGetContext(ListenerCallback, _listener);

            var request = context.Request;

            if (request.HttpMethod.Equals("GET"))
            {
                var match5M = RegEx5M.Match(request.Url.LocalPath);
                if (match5M.Success)
                {
                    HandleGetToken(match5M.Groups[1].Value, context.Request, context.Response);
                    return;
                }
            }

            Handle404(context.Response);
        }

        private static void Handle404(HttpListenerResponse response)
        {
            response.StatusCode = 404;
            response.Close();
        }

        private void HandleGetToken(string tokenClass, HttpListenerRequest request, HttpListenerResponse response)
        {
            var requestEssentials = HttpWebHelper.GetRequestEssentials(request);
            var responseEssentials = _actualPublicAsm.GetToken(tokenClass, requestEssentials);
            HttpWebHelper.PrepareResponse(responseEssentials, response);
            response.Close();
        }
    }
}