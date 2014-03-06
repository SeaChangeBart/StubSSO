using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using Common;

namespace WebASM
{
    public class PrivateWebAsm
    {
        private readonly Common.IServe4J _actual4J;
        private readonly Common.IServe4K _actual4K;

        public PrivateWebAsm ( Common.IServe4J actual4J, Common.IServe4K actual4K)
        {
            _actual4J = actual4J;
            _actual4K = actual4K;
        }

        public ResponseEssentials VerifyToken(string tokenClass, string tokenId)
        {
            var authenticationAndCacheTime = _actual4J.VerifyTokenGetValidity(tokenId, tokenClass);
            if (authenticationAndCacheTime == null)
                return new ResponseEssentials {StatusCode = HttpStatusCode.NotFound};

            if (authenticationAndCacheTime.Authentication == null)
                return new ResponseEssentials { StatusCode = HttpStatusCode.NotFound };

            var authString = Serializers.AuthenticationToXmlString(authenticationAndCacheTime.Authentication);
            var cacheTime = authenticationAndCacheTime.CacheTime;
            return new ResponseEssentials
                       {
                           StatusCode = HttpStatusCode.OK,
                           Mime = "application/xml",
                           Body = authString,
                           CacheHeader =
                               cacheTime.HasValue
                                   ? string.Format("max-age={0}", (int) cacheTime.Value.TotalSeconds)
                                   : "no-cache, no-store",
                       };
        }

        public ResponseEssentials InjectSession(string sessionXml)
        {
            try
            {
                var session = Serializers.SessionFromXmlStringSession(sessionXml);
                session.Expiration = session.Expiration.ToUniversalTime();

                ValidateSession(session);

                var seacToken = _actual4K.InjectSession(session);
                var responseBody = Serializers.SeacTokenToXmlString(seacToken);
                return new ResponseEssentials
                           {
                               Body = responseBody,
                               StatusCode = HttpStatusCode.OK,
                               Mime = "application/xml",
                           };
            }
            catch (Exception ex)
            {
                return new ResponseEssentials
                           {StatusCode = HttpStatusCode.BadRequest, Body = ex.Message, Mime = "text/plain"};
            }
        }

        private static void ValidateSession(Session session)
        {
            if (session==null)
                throw new ArgumentNullException("session");

            if (session.Expiration.Kind != DateTimeKind.Utc)
                throw new ArgumentException("Expiration must be in UTC", "session");

            if (session.Expiration < DateTime.UtcNow)
                throw new ArgumentException("Expiration must be in the future", "session");

            if (session.Authentication == null)
                throw new ArgumentException("No Authentication", "session");

            if (session.Authentication.CpeId != null && string.IsNullOrWhiteSpace(session.Authentication.CpeId))
                throw new ArgumentException("Empty CpeId is illegal", "session");

            if (session.Authentication.CustomerId != null && string.IsNullOrWhiteSpace(session.Authentication.CustomerId))
                throw new ArgumentException("Empty CustomerId is illegal", "session");

            if (session.Authentication.ProfileId != null && string.IsNullOrWhiteSpace(session.Authentication.ProfileId))
                throw new ArgumentException("Empty ProfileId is illegal", "session");

            if (session.Authentication.ProfileId==null && session.Authentication.CustomerId==null && session.Authentication.CpeId==null)
                throw new ArgumentException("Empty Authentication in Session");
        }
    }
}