using System.Net;
using System.Text;
using WebASM;

namespace HttpASM
{
    internal static class HttpWebHelper
    {
        public static RequestEssentials GetRequestEssentials( HttpListenerRequest req )
        {
            return new RequestEssentials { Headers = new WebHeaderCollection { req.Headers } };
        }

        public static void PrepareResponse(ResponseEssentials responseEssentials, HttpListenerResponse res)
        {
            res.ContentType = responseEssentials.Mime ?? "text/plain";

            res.StatusCode = (int)responseEssentials.StatusCode;

            if (responseEssentials.CacheHeader != null)
                res.Headers.Add(HttpResponseHeader.CacheControl, responseEssentials.CacheHeader);

            if (responseEssentials.WwwAuthenticate != null)
                res.AddHeader("WWW-Authenticate", responseEssentials.WwwAuthenticate);

            if (!string.IsNullOrEmpty(responseEssentials.Body))
            {
                res.ContentEncoding = Encoding.UTF8;
                var bytes = Encoding.UTF8.GetBytes(responseEssentials.Body);
                res.ContentLength64 = bytes.Length;
                res.OutputStream.Write(bytes, 0, bytes.Length);
                res.OutputStream.Close();
            }
        }
    }
}