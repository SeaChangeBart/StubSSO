using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Common;
using WebASM;

namespace HttpASM
{
    public class PrivateHttpAsm
    {
        private readonly PrivateWebAsm _actualPrivateAsm;
        private readonly HttpListener _listener;
        private static readonly Regex RegEx4J = new Regex("/Class/([^/]+)/Token/([^/]+)/Authentication$");
        private static readonly Regex RegEx4K = new Regex("/Session$");

        public PrivateHttpAsm(PrivateWebAsm actualPrivateAsm, string address)
        {
            _actualPrivateAsm = actualPrivateAsm;
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

        public PrivateHttpAsm(IServe4J actual4J, IServe4K actual4K, string address)
            : this(new PrivateWebAsm(actual4J, actual4K), address)
        {
        }

        private void ListenerCallback(IAsyncResult result)
        {
            var listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            var context = listener.EndGetContext(result);
            context.Response.KeepAlive = true;

            _listener.BeginGetContext(ListenerCallback, _listener);

            var request = context.Request;

            if (request.HttpMethod.Equals("GET"))
            {
                var match4J = RegEx4J.Match(request.Url.LocalPath);
                if (match4J.Success)
                {
                    HandleGetAuthenticationForToken(match4J.Groups[1].Value, match4J.Groups[2].Value, context.Response);
                    return;
                }
            }

            if (request.HttpMethod.Equals("POST"))
            {
                var match4K = RegEx4K.Match(request.Url.LocalPath);
                if (match4K.Success)
                {
                    HandleInjectSession(request.InputStream, context.Response);
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

        private void HandleGetAuthenticationForToken(string tokenClass, string tokenId, HttpListenerResponse response)
        {
            var responseEssentials = _actualPrivateAsm.VerifyToken(tokenClass, tokenId);
            HttpWebHelper.PrepareResponse(responseEssentials, response);
            response.Close();
        }

        private void HandleInjectSession(Stream bodyStream, HttpListenerResponse response)
        {
            using (var reader = new StreamReader(bodyStream))
            {
                var sessionXml = reader.ReadToEnd();
                HandleInjectSession(sessionXml, response);
            }
        }

        private void HandleInjectSession(string sessionXml, HttpListenerResponse response)
        {
            var responseEssentials = _actualPrivateAsm.InjectSession(sessionXml);
            HttpWebHelper.PrepareResponse(responseEssentials, response);
            response.Close();
        }
    }
}
