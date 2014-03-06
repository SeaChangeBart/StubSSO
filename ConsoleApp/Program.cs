using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using Common;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var asmInstance = new ASM.ASM("session", "personal", TimeSpan.FromMinutes(10), "action");
/*
            var asmInstance = new FakeASM((s, s1, arg3) => new SeacToken {Class = "personal", Expiration = DateTime.UtcNow.AddMinutes(30), SingleUse = false, Token = "0611393339"})
                                  {
                                      Response4J = new AuthenticationAndCacheTime {Authentication = new Authentication {CustomerId = "kust01"}, CacheTime = TimeSpan.FromMinutes(10)},
                                      ReturnValue4K = new SeacToken{ Class = "session", Expiration = DateTime.UtcNow.AddMinutes(30), SingleUse = false, Token = "0611393339"}
                                  };
 */
            var privateHost = new HttpASM.PrivateHttpAsm(asmInstance, asmInstance, "http://+:80/ASM/Private/");
            var publicHost = new HttpASM.PublicHttpAsm(asmInstance, "http://+:80/ASM/Public/");
            privateHost.Start();
            publicHost.Start();
            //var publicAddress = new Uri("http://0.0.0.0/ASM/Public/");
            //var privateAddress = new Uri("http://0.0.0.0/ASM/Private/");
            //var publicWcfHost = new WebServiceHost(new WCFASM.PublicWcfAsm(asmInstance), publicAddress);
            //var privateWcfHost = new WebServiceHost(new WCFASM.PrivateWcfAsm(asmInstance, asmInstance), privateAddress);
            //publicWcfHost.Open();
            //privateWcfHost.Open();
            Console.WriteLine("Press enter to quit");
            Console.ReadLine();
            //privateWcfHost.Close();
            //publicWcfHost.Close();
            publicHost.Stop();
            privateHost.Stop();
        }
    }

    class FakeASM : IServe5M,IServe4J,IServe4K
    {
        public SeacToken ReturnValue4K { get; set; }
        public Exception ExceptionToThrow4K { get; set; }
        public SeacToken InjectSession(Session session)
        {
            if (ExceptionToThrow4K != null)
                throw ExceptionToThrow4K;
            return ReturnValue4K;
        }
        
        private readonly Func<string, string, string, SeacToken> m_RetFunc5M;
        public FakeASM(Func<string, string, string, SeacToken> retFunc5M)
        {
            m_RetFunc5M = retFunc5M;
        }

        public SeacToken GetToken(string tokenClass, string inToken, string inTokenClass)
        {
            return m_RetFunc5M(tokenClass, inToken, inTokenClass);
        }

        public string Auth5LTokenClass
        {
            get { return "session"; }
        }

        public string Auth5LRealm
        {
            get { return "adrenalin"; }
        }

        public AuthenticationAndCacheTime Response4J { get; set; }
        public Authentication VerifyToken(string token, string tokenClass)
        {
            return Response4J.Authentication;
        }

        public AuthenticationAndCacheTime VerifyTokenGetValidity(string token, string tokenClass)
        {
            return Response4J;
        }
    }
}
