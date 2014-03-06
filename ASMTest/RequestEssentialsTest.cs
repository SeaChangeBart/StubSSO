using System.Net;
using Common;
using WebASM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ASMTest
{


    /// <summary>
    ///This is a test class for RequestEssentialsTest and is intended
    ///to contain all RequestEssentialsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RequestEssentialsTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
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


        /// <summary>
        ///A test for GetSeacTokenAndClass
        ///</summary>
        [TestMethod()]
        public void GetSeacTokenAndClassTest()
        {
            var target = new RequestEssentials {Headers = new WebHeaderCollection()};
            target.Headers.Add(HttpRequestHeader.Authorization, "basic 987ushdfuy23987498q279857y987987==");
            target.Headers.Add(HttpRequestHeader.Authorization, "SeacToken class=\"high\" token=\"0611393339\"");
            var expected = new SeacTokenBare {Token = "0611393339", Class = "high"};
            var actual = target.GetSeacTokenAndClass();
            Assert.AreEqual(expected.Token, actual.Token);
            Assert.AreEqual(expected.Class, actual.Class);
        }
    }
}
