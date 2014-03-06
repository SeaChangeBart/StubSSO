using ASM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Common;

namespace ASMTest
{
    
    
    /// <summary>
    ///This is a test class for ASMTest and is intended
    ///to contain all ASMTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ASMTest
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

        const String tokenClassSession = "session";
        const String tokenClassShort = "short";
        const String tokenClassSingleUse = "singleUse";
        static readonly TimeSpan shortTokenValidity = TimeSpan.FromSeconds(5);

        private ASM.ASM CreateASM()
        {
            return new ASM.ASM(tokenClassSession, tokenClassShort, shortTokenValidity, tokenClassSingleUse);
        }

        /// <summary>
        /// Fresh ASM must not verify any token
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_EmptyInputMustNotReturnAuth()
        {
            var target = CreateASM();
            string token = string.Empty;
            string tokenClass = string.Empty;
            var actual = target.VerifyToken(token, tokenClass);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for InjectSession
        ///</summary>
        [TestMethod()]
        public void InjectSessionTest_SuccessFullIngestTokenMustBeValid()
        {
            var target = CreateASM();
            var auth = new Authentication{ CustomerId = "testCustomer" };
            var session = new Session { Authentication = auth, Expiration = DateTime.UtcNow.AddHours(1) };
            var actual = target.InjectSession(session);
            Assert.IsNotNull(actual);
            Assert.AreEqual(tokenClassSession, actual.Class);
            Assert.IsNotNull(actual.Token);
            Assert.AreEqual(session.Expiration, actual.Expiration);
            Assert.IsFalse(actual.SingleUse);
        }

        /// <summary>
        ///A test for InjectSession
        ///</summary>
        [TestMethod()]
        public void InjectSessionTest_SuccessFullIngestTokenMustBeUsable()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var token = target.InjectSession(session);
            var actual = target.VerifyToken(token.Token, token.Class);
            Assert.AreEqual<Authentication>(expected, actual);
        }

        /// <summary>
        ///A test for InjectSession
        ///</summary>
        [TestMethod()]
        public void InjectSessionTest_SuccessFullIngestTokenMustBeUsableToObtainToken()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var singleUseToken = target.GetToken(tokenClassSingleUse, sessionToken.Token, sessionToken.Class);
            Assert.IsTrue(singleUseToken.SingleUse);
            var actual = target.VerifyToken(singleUseToken.Token, singleUseToken.Class);
            Assert.AreEqual<Authentication>(expected, actual);
        }

        /// <summary>
        ///A test for VerifyToken
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_SingleUseTokenMustBeSingleUse()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var singleUseToken = target.GetToken(tokenClassSingleUse, sessionToken.Token, sessionToken.Class);
            var validAuth = target.VerifyToken(singleUseToken.Token, singleUseToken.Class);
            Assert.AreEqual(validAuth, expected);
            var actual = target.VerifyToken(singleUseToken.Token, singleUseToken.Class);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for VerifyToken
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_SingleUseTokenMustHaveNoCacheTime()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var singleUseToken = target.GetToken(tokenClassSingleUse, sessionToken.Token, sessionToken.Class);
            var validAuth = target.VerifyTokenGetValidity(singleUseToken.Token, singleUseToken.Class);
            Assert.AreEqual(validAuth.Authentication, expected);
            Assert.IsFalse(validAuth.CacheTime.HasValue);
            var actual = target.VerifyToken(singleUseToken.Token, singleUseToken.Class);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for VerifyToken
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_ShortTokenMustBeValid()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var browseToken = target.GetToken(tokenClassShort, sessionToken.Token, sessionToken.Class);
            var actual = target.VerifyToken(browseToken.Token, browseToken.Class);
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        ///A test for VerifyToken
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_ShortTokenMustHaveExpirationTime()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var browseToken = target.GetToken(tokenClassShort, sessionToken.Token, sessionToken.Class);
            var actual = target.VerifyTokenGetValidity(browseToken.Token, browseToken.Class);
            Assert.AreEqual(actual.Authentication, expected);
            Assert.IsTrue(actual.CacheTime.HasValue);
        }

        /// <summary>
        ///A test for VerifyToken
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_ShortTokenMustBeValidTwice()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var browseToken = target.GetToken(tokenClassShort, sessionToken.Token, sessionToken.Class);
            var firstAuth = target.VerifyToken(browseToken.Token, browseToken.Class);
            var actual = target.VerifyToken(browseToken.Token, browseToken.Class);
            Assert.AreEqual(actual, expected);
        }

        /// <summary>
        ///A test for VerifyToken
        ///</summary>
        [TestMethod()]
        public void VerifyTokenTest_ShortTokenMustBeInvalidAfterExpiration()
        {
            var target = CreateASM();
            var expected = new Authentication { CustomerId = "testCustomer" };
            var session = new Session { Authentication = expected, Expiration = DateTime.UtcNow.AddHours(1) };
            var sessionToken = target.InjectSession(session);
            var browseToken = target.GetToken(tokenClassShort, sessionToken.Token, sessionToken.Class);
            var firstAuth = target.VerifyToken(browseToken.Token, browseToken.Class);
            System.Threading.Thread.Sleep(shortTokenValidity);
            System.Threading.Thread.Sleep(100);
            var actual = target.VerifyToken(browseToken.Token, browseToken.Class);
            Assert.IsNull(actual);
        }
    }
}
