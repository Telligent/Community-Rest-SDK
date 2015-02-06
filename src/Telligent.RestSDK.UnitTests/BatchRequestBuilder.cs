using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Moq;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Rest.SDK.Model;
using Telligent.RestSDK.IntegrationTests;

namespace Telligent.RestSDK.UnitTests
{
    [TestFixture]
    public class BatchRequestBuilder
    {
        // private IRest rest;
        // private IRestCommunicationProxy _proxy;
        [TestFixtureSetUp]
        public void Setup()
        {
            //  IRest = new Mock<IRestCommunicationProxy>
        }
        [Test]
        public async Task batch_url_is_correct()
        {
            string url = null;
            var proxy =
                new Mock<IRestCommunicationProxy>();
            proxy.Setup(
                    m =>
                        m.Post(It.IsAny<RestHost>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<Action<HttpWebRequest>>())
                            )
                    .Returns<RestHost, string, string,HttpPostedFile,Action<HttpWebRequest>>(
                        (h, u, data, req, res) =>
                        {
                            url = u;
                            return Task.FromResult("{\"Response\":\"Ok\"}");

                        });

            IRest rest = new Evolution.RestSDK.Implementations.Rest(proxy.Object);
            var requests = new List<BatchRequest>();
            var nvc = new NameValueCollection();
            nvc.Add("parm", "p1");
            requests.Add(new BatchRequest("info.json", 0) { ApiVersion = 2, RequestParameters = nvc });
            var nvc2 = new NameValueCollection();
            nvc2.Add("parm", "p1");
            nvc2.Add("parm2", "p2");
            requests.Add(new BatchRequest("users.json", 0) { ApiVersion = 2, RequestParameters = nvc2 });
            var resp = await rest.BatchEndpointString(new TestRestHost("http://community/"), 2, requests);

            Assert.AreEqual("http://community/api.ashx/v2/batch.json", url);

        }
        [Test]
        public async Task batch_postdata_requests_is_correct()
        {
            string postData = null;
            var proxy =
                new Mock<IRestCommunicationProxy>();
            proxy.Setup(
                    m =>
                        m.Post(It.IsAny<RestHost>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<Action<HttpWebRequest>>())
                            )
                    .Returns<RestHost, string, string, HttpPostedFile, Action<HttpWebRequest>>(
                        (h, u, data, req, res) =>
                        {
                            postData  = data;
                            return Task.FromResult("{\"Response\":\"Ok\"}");

                        });

            IRest rest = new Evolution.RestSDK.Implementations.Rest(proxy.Object);
            var requests = new List<BatchRequest>();
            var nvc = new NameValueCollection();
            nvc.Add("parm", "p1");
            requests.Add(new BatchRequest("info.json", 0) { ApiVersion = 2, RequestParameters = nvc });
            var nvc2 = new NameValueCollection();
            nvc2.Add("parm", "p1");
            nvc2.Add("parm2", "p2");
            requests.Add(new BatchRequest("users.json", 1) { ApiVersion = 2, RequestParameters = nvc2 });
            var resp = await rest.BatchEndpointString(new TestRestHost("http://community/"), 2, requests);

            var reqParms = HttpUtility.ParseQueryString(postData);
            Assert.AreEqual(7, reqParms.Keys.Count);
            Assert.AreEqual("false", reqParms["Sequential"]);
            //7 = 3 per request plus sequentially
        }
        [Test]
        public async Task batch_postdata_requests_is_correct_sequentially()
        {
            string postData = null;
            var proxy =
                new Mock<IRestCommunicationProxy>();
            proxy.Setup(
                    m =>
                        m.Post(It.IsAny<RestHost>(), It.IsAny<string>(), It.IsAny<string>(), null, It.IsAny<Action<HttpWebRequest>>())
                            )
                    .Returns<RestHost, string, string, HttpPostedFile, Action<HttpWebRequest>>(
                        (h, u, data, req, res) =>
                        {
                            postData = data;
                            return Task.FromResult("{\"Response\":\"Ok\"}");

                        });

            IRest rest = new Evolution.RestSDK.Implementations.Rest(proxy.Object);
            var requests = new List<BatchRequest>();
            var nvc = new NameValueCollection();
            nvc.Add("parm", "p1");
            requests.Add(new BatchRequest("info.json", 0) { ApiVersion = 2, RequestParameters = nvc });
            var nvc2 = new NameValueCollection();
            nvc2.Add("parm", "p1");
            nvc2.Add("parm2", "p2");
            requests.Add(new BatchRequest("users.json", 1) { ApiVersion = 2, RequestParameters = nvc2 });
            var resp = await rest.BatchEndpointString(new TestRestHost("http://community/"), 2, requests, true, new BatchRequestOptions() { RunSequentially = true });

            var reqParms = HttpUtility.ParseQueryString(postData);
            Assert.AreEqual(7, reqParms.Keys.Count);
            Assert.AreEqual("true", reqParms["Sequential"]);
            //7 = 3 per request plus sequentially
        }
    }
}

