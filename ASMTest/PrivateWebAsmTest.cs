using System.Globalization;
using WebASM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Common;

namespace ASMTest
{
    internal class StubServe4K : IServe4K
    {
        public SeacToken ReturnValue { get; set; }
        public Exception ExceptionToThrow { get; set; }
        public SeacToken InjectSession(Session session)
        {
            if (ExceptionToThrow != null)
                throw ExceptionToThrow;
            return ReturnValue;
        }
    }

    internal class StubServe4J : IServe4J
    {
        public Authentication ResponseAuthentication4J { get; set; }
        public TimeSpan? ResponseCacheTime4J { get; set; }
        public Authentication VerifyToken(string token, string tokenClass)
        {
            return ResponseAuthentication4J;
        }

        public AuthenticationAndCacheTime VerifyTokenGetValidity(string token, string tokenClass)
        {
            return new AuthenticationAndCacheTime
                       {
                           Authentication = ResponseAuthentication4J,
                           CacheTime = ResponseCacheTime4J
                       };
        }
    }

    /// <summary>
    ///This is a test class for PrivateWebAsmTest and is intended
    ///to contain all PrivateWebAsmTest Unit Tests
    ///</summary>
    [TestClass]
    public class PrivateWebAsmTest
    {
        /// <summary>
        ///A test for AuthenticationToString
        ///</summary>
        [TestMethod]
        [DeploymentItem("WebASM.dll")]
        public void AuthenticationToStringTest()
        {
            var authenication = new Authentication {CustomerId = "test"};
            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Authentication xmlns=\"urn:schange:userauth:1.0\"><CustomerId>test</CustomerId></Authentication>";
            var actual = Serializers_Accessor.AuthenticationToXmlString(authenication);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SeacTokenToString
        ///</summary>
        [TestMethod]
        [DeploymentItem("WebASM.dll")]
        public void SeacTokenToStringTest()
        {
            var seacToken = new SeacToken
                                      {
                                          Class = "klas",
                                          Expiration =
                                              DateTime.Parse("2010-05-05T12:00:00Z", null, DateTimeStyles.RoundtripKind),
                                          SingleUse = false,
                                          Token = "0611393339"
                                      };
            const string expected = "<?xml version=\"1.0\" encoding=\"utf-8\"?><SeacToken class=\"klas\" expiration=\"2010-05-05T12:00:00Z\" singleUse=\"false\" xmlns=\"urn:schange:userauth:1.0\">0611393339</SeacToken>";
            var actual = Serializers_Accessor.SeacTokenToXmlString(seacToken);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void InjectSessionTestNoNamespaceXml()
        {
            var actual4K = new StubServe4K
                                    {
                                        ReturnValue =
                                            new SeacToken
                                                {
                                                    Class = "klas",
                                                    Token = "0611393339",
                                                    Expiration =
                                                        DateTime.Parse("2019-05-05T12:00:00Z", null,
                                                                       DateTimeStyles.RoundtripKind),
                                                    SingleUse = false
                                                }
                                    };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<Session><Expiration>2019-05-05T13:00:00+01:00</Expiration><Authentication><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            const string expectedBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?><SeacToken class=\"klas\" expiration=\"2019-05-05T12:00:00Z\" singleUse=\"false\" xmlns=\"urn:schange:userauth:1.0\">0611393339</SeacToken>";
            Assert.AreEqual(System.Net.HttpStatusCode.OK, actual.StatusCode);
            Assert.AreEqual(expectedBody, actual.Body);
            Assert.AreEqual("application/xml", actual.Mime);
        }

        [TestMethod]
        public void InjectSessionTestExpirationInPast()
        {
            var actual4K = new StubServe4K
            {
                ReturnValue =
                    new SeacToken
                    {
                        Class = "klas",
                        Token = "0611393339",
                        Expiration =
                            DateTime.Parse("2010-05-05T12:00:00Z", null,
                                           DateTimeStyles.RoundtripKind),
                        SingleUse = false
                    }
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<Session><Expiration>2010-05-05T13:00:00+01:00</Expiration><Authentication><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod]
        public void InjectSessionTestNamespaceXml()
        {
            const string dateString = "2016-05-05T12:00:00Z";
            var actual4K = new StubServe4K
            {
                ReturnValue =
                    new SeacToken
                    {
                        Class = "klas",
                        Token = "0611393339",
                        Expiration =
                            DateTime.Parse(dateString, null,
                                           DateTimeStyles.RoundtripKind),
                        SingleUse = false
                    }
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Expiration>" + dateString + "</Expiration><Authentication><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            const string expectedBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?><SeacToken class=\"klas\" expiration=\"" + dateString + "\" singleUse=\"false\" xmlns=\"urn:schange:userauth:1.0\">0611393339</SeacToken>";
            Assert.AreEqual(System.Net.HttpStatusCode.OK, actual.StatusCode);
            Assert.AreEqual(expectedBody, actual.Body);
            Assert.AreEqual("application/xml", actual.Mime);
        }

        [TestMethod]
        public void InjectSessionTest_4KReturnsNull()
        {
            var actual4K = new StubServe4K
            {
                ReturnValue = null
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Expiration>2010-05-05T13:00:00+01:00</Expiration><Authentication><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod]
        public void InjectSessionTest_4KThrowsArgumentException()
        {
            var actual4K = new StubServe4K
            {
                ExceptionToThrow = new ArgumentException("whatever")
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Expiration>2010-05-05T13:00:00+01:00</Expiration><Authentication><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod]
        public void InjectSessionTestNoExpirationGiven()
        {
            var actual4K = new StubServe4K
            {
                ReturnValue =
                    new SeacToken
                    {
                        Class = "klas",
                        Token = "0611393339",
                        Expiration =
                            DateTime.Parse("2010-05-05T12:00:00Z", null,
                                           DateTimeStyles.RoundtripKind),
                        SingleUse = false
                    }
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Authentication><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod]
        public void InjectSessionTestIllegalXml()
        {
            var actual4K = new StubServe4K
            {
                ReturnValue =
                    new SeacToken
                    {
                        Class = "klas",
                        Token = "0611393339",
                        Expiration =
                            DateTime.Parse("2010-05-05T12:00:00Z", null,
                                           DateTimeStyles.RoundtripKind),
                        SingleUse = false
                    }
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Authentications><CustomerId>kust01</CustomerId></Authentication></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod]
        public void InjectSessionTestNoAuthGiven()
        {
            var actual4K = new StubServe4K
            {
                ReturnValue =
                    new SeacToken
                    {
                        Class = "klas",
                        Token = "0611393339",
                        Expiration =
                            DateTime.Parse("2010-05-05T12:00:00Z", null,
                                           DateTimeStyles.RoundtripKind),
                        SingleUse = false
                    }
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Expiration>2010-05-05T13:00:00+01:00</Expiration></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod]
        public void InjectSessionTestEmptyAuthGiven()
        {
            var actual4K = new StubServe4K
            {
                ReturnValue =
                    new SeacToken
                    {
                        Class = "klas",
                        Token = "0611393339",
                        Expiration =
                            DateTime.Parse("2010-05-05T12:00:00Z", null,
                                           DateTimeStyles.RoundtripKind),
                        SingleUse = false
                    }
            };
            var target = new PrivateWebAsm(null, actual4K);
            const string sessionXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Session xmlns=\"urn:schange:userauth:1.0\"><Authentication><customerId>kust01</customerId></Authentication><Expiration>2010-05-05T13:00:00+01:00</Expiration></Session>";
            var actual = target.InjectSession(sessionXml);
            Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, actual.StatusCode);
        }

        [TestMethod()]
        public void VerifyTokenTestNotFound()
        {
            IServe4J actual4J = new StubServe4J();
            var target = new PrivateWebAsm(actual4J, null);
            var tokenClass = string.Empty;
            var tokenId = string.Empty;
            var actual = target.VerifyToken(tokenClass, tokenId);
            Assert.IsNotNull(actual);
            Assert.AreEqual(System.Net.HttpStatusCode.NotFound,actual.StatusCode);
        }

        [TestMethod()]
        public void VerifyTokenTestReusableOK()
        {
            IServe4J actual4J = new StubServe4J
                                    {
                                        ResponseAuthentication4J = new Authentication
                                                                       {
                                                                           CustomerId = "testCustomer"
                                                                       },
                                        ResponseCacheTime4J = TimeSpan.FromMinutes(1)
                                    };
            var target = new PrivateWebAsm(actual4J, null);
            var tokenClass = string.Empty;
            var tokenId = string.Empty;
            var actual = target.VerifyToken(tokenClass, tokenId);
            const string expectedBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Authentication xmlns=\"urn:schange:userauth:1.0\"><CustomerId>testCustomer</CustomerId></Authentication>";
            const string expectedCacheHeader = "max-age=60";
            Assert.IsNotNull(actual);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, actual.StatusCode);
            Assert.AreEqual(expectedBody, actual.Body);
            Assert.AreEqual(expectedCacheHeader, actual.CacheHeader);
            Assert.AreEqual("application/xml", actual.Mime);
        }

        [TestMethod()]
        public void VerifyTokenTestSingleUseOK()
        {
            IServe4J actual4J = new StubServe4J
            {
                ResponseAuthentication4J = new Authentication
                {
                    CustomerId = "testCustomer"
                },
            };
            var target = new PrivateWebAsm(actual4J, null);
            var tokenClass = string.Empty;
            var tokenId = string.Empty;
            var actual = target.VerifyToken(tokenClass, tokenId);
            const string expectedBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Authentication xmlns=\"urn:schange:userauth:1.0\"><CustomerId>testCustomer</CustomerId></Authentication>";
            const string expectedCacheHeader = "no-cache, no-store";
            Assert.IsNotNull(actual);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, actual.StatusCode);
            Assert.AreEqual(expectedBody, actual.Body);
            Assert.AreEqual(expectedCacheHeader, actual.CacheHeader);
            Assert.AreEqual("application/xml", actual.Mime);
        }
    }
}
