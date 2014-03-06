using System;
using System.Net;

namespace WebASM
{
    public class PublicWebAsm
    {
        private readonly Common.IServe5M _actual5M;
        public PublicWebAsm(Common.IServe5M actual5M)
        {
            _actual5M = actual5M;
        }

        public ResponseEssentials GetToken(string tokenClass, RequestEssentials requestEssentials)
        {
            var seacTokenBare = requestEssentials.GetSeacTokenAndClass();
            if (seacTokenBare == null)
                return NotAuthorizedResponse();

            var inToken = seacTokenBare.Token;
            var inClass = seacTokenBare.Class;

            Common.SeacToken seacToken;
            try
            {
                seacToken = _actual5M.GetToken(tokenClass, inToken, inClass);
            }
            catch (UnauthorizedAccessException)
            {
                return NotAuthorizedResponse();
            }

            if (seacToken==null)
                return NotFoundResponse();

            var responseBody = Serializers.SeacTokenToXmlString(seacToken);
            return new ResponseEssentials
            {
                Body = responseBody,
                StatusCode = HttpStatusCode.OK,
                Mime = "application/xml",
            };
        }

        private ResponseEssentials NotFoundResponse()
        {
            return new ResponseEssentials
            {
                StatusCode = HttpStatusCode.NotFound
            };
        }

        private ResponseEssentials NotAuthorizedResponse()
        {
            return new ResponseEssentials
                       {
                           WwwAuthenticate =
                               string.Format("SeacToken class=\"{0}\" realm=\"{1}\"", _actual5M.Auth5LTokenClass,
                                             _actual5M.Auth5LRealm),
                           StatusCode = HttpStatusCode.Unauthorized
                       };
        }
    }
}
