using System.Globalization;
using System.Net;
using WebASM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Common;

namespace ASMTest
{
    class StubServe5M : IServe5M
    {
        private readonly Func<string, string, string, SeacToken> m_RetFunc;
        public StubServe5M(Func<string, string, string, SeacToken> retFunc)
        {
            m_RetFunc = retFunc;
        }

        public SeacToken GetToken(string tokenClass, string inToken, string inTokenClass)
        {
            return m_RetFunc(tokenClass, inToken, inTokenClass);
        }

        public string Auth5LTokenClass
        {
            get { return "session"; }
        }

        public string Auth5LRealm
        {
            get { return "adrenalin"; }
        }
    }

    /// <summary>
    ///This is a test class for PublicWebAsmTest and is intended
    ///to contain all PublicWebAsmTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PublicWebAsmTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        [TestMethod()]
        public void GetTokenTest()
        {
            IServe5M actual5M = new StubServe5M((reqClass, a, b) => new SeacToken
            {
                Class = reqClass,
                Expiration = DateTime.Parse("2011-02-02T12:00:00Z", null, DateTimeStyles.RoundtripKind),
                SingleUse = false,
                Token = "0611393339"
            });
            const string expectedBody = "<?xml version=\"1.0\" encoding=\"utf-8\"?><SeacToken class=\"browse\" expiration=\"2011-02-02T12:00:00Z\" singleUse=\"false\" xmlns=\"urn:schange:userauth:1.0\">0611393339</SeacToken>";

            var target = new PublicWebAsm(actual5M);

            var response = target.GetToken("browse",
                            new RequestEssentials
                            {
                                Headers =
                                    new WebHeaderCollection { "Authorization: SeacToken token=\"1\" class=\"class\"" }
                            });

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Mime, "application/xml");
            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(response.Body, expectedBody);
            Assert.IsNull(response.WwwAuthenticate);
        }

        [TestMethod()]
        public void GetTokenTestWrongAuth()
        {
            IServe5M actual5M = new StubServe5M( (reqClass, a,b) => new SeacToken
                            {
                                Class = reqClass,
                                Expiration = DateTime.Parse("2011-02-02T12:00:00Z", null, DateTimeStyles.RoundtripKind),
                                SingleUse = false,
                                Token = "0611393339"
                            } );
            var target = new PublicWebAsm(actual5M);

            var response = target.GetToken("browse",
                            new RequestEssentials
                                {
                                    Headers =
                                        new WebHeaderCollection {"Authorization: SeacTokens token=\"1\" class=\"class\""}
                                });

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual("SeacToken class=\"session\" realm=\"adrenalin\"", response.WwwAuthenticate);
            Assert.IsNotNull(response.WwwAuthenticate);
        }

        [TestMethod()]
        public void GetTokenTest_NoAuth()
        {
            IServe5M actual5M = new StubServe5M((reqClass, a, b) => new SeacToken
                                                                        {
                                                                            Class = reqClass,
                                                                            Expiration =
                                                                                DateTime.Parse("2011-02-02T12:00:00Z",
                                                                                               null,
                                                                                               DateTimeStyles.
                                                                                                   RoundtripKind),
                                                                            SingleUse = false,
                                                                            Token = "0611393339"
                                                                        });
            var target = new PublicWebAsm(actual5M);

            var response = target.GetToken("browse",
                                           new RequestEssentials
                                               {
                                                   Headers =
                                                       new WebHeaderCollection {"Authorization: Digest kjahsdkjhasd"}
                                               });

            Assert.IsNotNull(response);
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.AreEqual("SeacToken class=\"session\" realm=\"adrenalin\"", response.WwwAuthenticate);
            Assert.IsNotNull(response.WwwAuthenticate);
        }
    }
}
