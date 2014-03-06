using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class AuthenticationAndCacheTime
    {
        public Authentication Authentication { get; set; }
        public TimeSpan? CacheTime { get; set; }
    }
    public interface IServe4J
    {
        Authentication VerifyToken(string token, string tokenClass);
        AuthenticationAndCacheTime VerifyTokenGetValidity(string token, string tokenClass);
    }
}
