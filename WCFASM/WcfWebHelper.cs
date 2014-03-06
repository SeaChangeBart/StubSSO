using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using WebASM;

namespace WCFASM
{
    internal static class WcfWebHelper
    {
        public static RequestEssentials GetRequestEssentials()
        {
            if (WebOperationContext.Current == null)
                throw new InvalidOperationException("WebOperationContext must not be null");

            return new RequestEssentials {Headers = WebOperationContext.Current.IncomingRequest.Headers};
        }

        public static Stream PrepareResponse(ResponseEssentials responseEssentials)
        {
            if (WebOperationContext.Current == null)
                throw new InvalidOperationException("WebOperationContext must not be null");

            WebOperationContext.Current.OutgoingResponse.ContentType = responseEssentials.Mime ?? "text/plain";

            WebOperationContext.Current.OutgoingResponse.StatusCode = responseEssentials.StatusCode;

            if (responseEssentials.CacheHeader != null)
                WebOperationContext.Current.OutgoingResponse.Headers.Add(HttpResponseHeader.CacheControl, responseEssentials.CacheHeader);

//            if (responseEssentials.WwwAuthenticate != null)
//                WebOperationContext.Current.OutgoingResponse.Headers.Add("WWW-Authenticate", responseEssentials.WwwAuthenticate);

            return StringStream(responseEssentials.Body);
        }

        private static Stream StringStream(string body)
        {
            Debug.Assert(WebOperationContext.Current != null, "WebOperationContext.Current != null");

            if (body == null)
                return null;

            var bytes = Encoding.UTF8.GetBytes(body);
            WebOperationContext.Current.OutgoingResponse.ContentLength = bytes.Length;
            return new MemoryStream(bytes);
        }
    }
}