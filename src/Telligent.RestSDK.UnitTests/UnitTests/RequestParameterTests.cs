using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Telligent.Rest.SDK.Model;

namespace Telligent.RestSDK.UnitTests.UnitTests
{
    [TestFixture]
    public class RequestParameterTests
    {
        private IRest _rest;
        [TestFixtureSetUp]
        public void Setup()
        {
            _rest = new  Evolution.RestSDK.Implementations.Rest(new Mock<IRestCommunicationProxy>().Object);

        }

        [Test]
        public void url_tokens_are_replaced_with_values_and_keys_cleared()
        {
            var nvc = new NameValueCollection();
            nvc.Add("userid","9999");
            nvc.Add("groupid", "1234");
            nvc.Add("appKey","some-cool-post");

            var url = "forums/{groupid}/thread/{appKey}/{userid}.json";

            var newUrl = _rest.ReplaceTokens(url, nvc);
            Assert.AreEqual("forums/1234/thread/some-cool-post/9999.json",newUrl);
          
        }
       
        [Test]
        public void url_tokens_are_replaced_with_values_and_non_specified_key_remains()
        {
            var nvc = new NameValueCollection();
            nvc.Add("userid", "9999");
            nvc.Add("groupid", "1234");


            var url = "forums/{groupid}/thread/{appKey}/foo/{bar}/{userid}.json";

            var newUrl = _rest.ReplaceTokens(url, nvc);
            Assert.AreEqual("forums/1234/thread/{appKey}/foo/{bar}/9999.json", newUrl);

        }

        [Test]
        public void url_query_string_is_created_no_existing_querystring()
        {
            var nvc = new NameValueCollection();
            nvc.Add("userid", "9999");
            nvc.Add("groupid", "1234");


            var url = "some/api/url.json";

            var newUrl = _rest.BuildQueryString(url, nvc);
            Assert.AreEqual("some/api/url.json?userid=9999&groupid=1234", newUrl);
        }
        [Test]
        public void url_query_string_is_created_existing_querystring()
        {
            var nvc = new NameValueCollection();
            nvc.Add("userid", "9999");
            nvc.Add("groupid", "1234");


            var url = "some/api/url.json?groupid=8888";

            var newUrl = _rest.BuildQueryString(url, nvc);
            Assert.AreEqual("some/api/url.json?groupid=8888&userid=9999&groupid=1234", newUrl);
        }
    }
}
