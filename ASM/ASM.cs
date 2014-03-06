using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Common;

namespace ASM
{
    public class ASM : Common.IServe4J, Common.IServe4K, Common.IServe5M
    {
        private class ClassParameters
        {
            public TimeSpan Validity { get; set; }
            public bool SingleUse { get; set; }
        }

        private class AuthenticationWithExpiration
        {
            public DateTime Expiration { get; set; }
            public Common.Authentication Authentication { get; set; }
        }

        private readonly string _sessionClass;
        private readonly Dictionary<string, ClassParameters> _classConfiguration = new Dictionary<string, ClassParameters>();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, AuthenticationWithExpiration>> _classes = new ConcurrentDictionary<string, ConcurrentDictionary<string, AuthenticationWithExpiration>>();
        private readonly TimeSpan _maxCacheTime = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _minCacheTime = TimeSpan.FromSeconds(2);

        private string GenerateTokenString()
        {
            return Guid.NewGuid().ToString();
        }

        public ASM(string sessionClass, string browseClass, TimeSpan browseTokenValidity, string singleUseClass)
        {
            _sessionClass = sessionClass;
            _classConfiguration[browseClass] = new ClassParameters { Validity = browseTokenValidity, SingleUse = false };
            _classConfiguration[singleUseClass] = new ClassParameters { Validity = browseTokenValidity, SingleUse = true };
        }

        public string Auth5LTokenClass
        {
            get { return _sessionClass; }
        }

        public string Auth5LRealm
        {
            get { return "adrenalin"; }
        }

        public AuthenticationAndCacheTime VerifyTokenGetValidity(string token, string tokenClass)
        {
            var singleUse = _classConfiguration.ContainsKey(tokenClass) && _classConfiguration[tokenClass].SingleUse;
            var authWithExpiration = singleUse
                                         ? VerifySingleUseToken(token, tokenClass)
                                         : VerifyMultiUseToken(token, tokenClass);
            if (authWithExpiration == null)
                return null;

            TimeSpan? cacheTime = null;

            if (!singleUse)
            {
                cacheTime = authWithExpiration.Expiration - DateTime.UtcNow;
                if (cacheTime.Value > _maxCacheTime)
                    cacheTime = _maxCacheTime;
                if (cacheTime.Value < _minCacheTime)
                    cacheTime = null;
            }

            return new AuthenticationAndCacheTime
                       {Authentication = authWithExpiration.Authentication, CacheTime = cacheTime};
        }

        public Common.Authentication VerifyToken(string token, string tokenClass)
        {
            var singleUse = _classConfiguration.ContainsKey(tokenClass) && _classConfiguration[tokenClass].SingleUse;
            var authWithExpiration = singleUse ? VerifySingleUseToken(token, tokenClass) : VerifyMultiUseToken(token, tokenClass);
            if (authWithExpiration == null)
                return null;
            return authWithExpiration.Authentication;
        }

        private AuthenticationWithExpiration VerifySingleUseToken(string token, string tokenClass)
        {
            var classDictionary = TryGetClassDictionary(tokenClass);
            if (classDictionary == null)
                return null;

            AuthenticationWithExpiration authWithExpiration;
            if (!classDictionary.TryRemove(token, out authWithExpiration))
                return null;

            if (DateTime.UtcNow > authWithExpiration.Expiration)
                return null;

            return authWithExpiration;
        }

        private AuthenticationWithExpiration VerifyMultiUseToken(string token, string tokenClass)
        {
            var classDictionary = TryGetClassDictionary(tokenClass);
            if (classDictionary == null)
                return null;

            AuthenticationWithExpiration authWithExpiration;
            if (!classDictionary.TryGetValue(token, out authWithExpiration))
                return null;

            if (authWithExpiration.Expiration <= DateTime.UtcNow)
            {
                classDictionary.TryRemove(token, out authWithExpiration);
                return null;
            }

            return authWithExpiration;
        }

        public Common.SeacToken InjectSession(Common.Session session)
        {
            if (session.Expiration.ToUniversalTime() <= DateTime.UtcNow)
                throw new ArgumentOutOfRangeException("session", "session.Expiration must be in the future");

            return InjectReusableTokenAuth(_sessionClass, session.Expiration, session.Authentication);
        }

        private Common.SeacToken InjectReusableTokenAuth(string tokenClass, DateTime expiration, Common.Authentication authentication)
        {
            var retVal = new Common.SeacToken { Class = tokenClass, Expiration = expiration, SingleUse = false, Token = GenerateTokenString() };
            StoreTokenAuth(tokenClass, retVal, authentication);
            return retVal;
        }

        private Common.SeacToken InjectSingleUseTokenAuth(string tokenClass, DateTime expiration, Common.Authentication authentication)
        {
            var retVal = new Common.SeacToken { Class = tokenClass, Expiration = expiration, SingleUse = true, Token = GenerateTokenString() };
            StoreTokenAuth(tokenClass, retVal, authentication);
            return retVal;
        }

        private void StoreTokenAuth(string tokenClass, Common.SeacToken seacToken, Common.Authentication authentication)
        {
            var classDictionary = GetOrCreateClassDictionary(tokenClass);
            classDictionary[seacToken.Token] = new AuthenticationWithExpiration { Expiration = seacToken.Expiration, Authentication = authentication };
        }

        private ConcurrentDictionary<string, AuthenticationWithExpiration> GetOrCreateClassDictionary(string tokenClass)
        {
            return _classes.GetOrAdd(tokenClass, _ => new ConcurrentDictionary<string, AuthenticationWithExpiration>());
        }

        private ConcurrentDictionary<string, AuthenticationWithExpiration> TryGetClassDictionary(string tokenClass)
        {
            ConcurrentDictionary<string, AuthenticationWithExpiration> retVal;
            if (!_classes.TryGetValue(tokenClass, out retVal))
                retVal = null;
            return retVal;
        }

        public Common.SeacToken GetToken(string tokenClass, string inToken, string inClass)
        {
            if (!inClass.Equals(_sessionClass))
                throw new UnauthorizedAccessException(_sessionClass);

            var authentication = VerifyToken(inToken, inClass);
            if (authentication == null)
                throw new UnauthorizedAccessException(_sessionClass);

            return CreateToken(tokenClass, authentication);
        }

        private Common.SeacToken CreateToken(string tokenClass, Common.Authentication authentication)
        {
            ClassParameters classConfig;
            if (!_classConfiguration.TryGetValue(tokenClass, out classConfig))
                return null;
            if (classConfig.SingleUse)
                return InjectSingleUseTokenAuth(tokenClass, DateTime.UtcNow.Add(classConfig.Validity), authentication);
            return InjectReusableTokenAuth(tokenClass, DateTime.UtcNow.Add(classConfig.Validity), authentication);
        }
    }
}
