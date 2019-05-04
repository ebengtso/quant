using System;
using System.IO;
using NUnit.Framework;
namespace AlgorithmUtils.Tests
{
     public class UtilsTests
    {

        public UtilsTests()
        {
        }

        [SetUp]
        public void SetUp()
        {

            Utils.emailAccount = TestConfiguration.Parameters["emailAccount"];
            Utils.emailPassword = TestConfiguration.Parameters["emailPassword"];
            Utils.SlackURL = TestConfiguration.Parameters["SlackURL"];
            Utils.SlackURL2 = TestConfiguration.Parameters["SlackURL2"];
            Utils.ID = "AlgoLucy";
        }
        [Test]
        public void SendSlackImage()
        {
        
            var result = Utils.SendSlack(Utils.ID, "test", "Hello test!", false);

            Assert.IsTrue(result);

        }

        [Test]
        public void SendSlack()
        {

            var result = Utils.SendSlackImage(Utils.ID, "test", "http://oanda.autochartist.com/aclite/imageViewer?type=CPPatternImage&uid=464650429&brokerid=232&w=1500&h=800&priceadjustment=0.0&instrumentid=0&offset=1", false);

            Assert.IsTrue(result);

        }
    }
}