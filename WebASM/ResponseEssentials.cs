using System.Net;

namespace WebASM
{
    public class ResponseEssentials
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Mime { get; set; }
        public string CacheHeader { get; set; }
        public string Body { get; set; }
        public string WwwAuthenticate { get; set; }
    }
}