using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.RestSDK.UnitTests.UnitTests
{
    [TestFixture]
   public class BatchRequestConstruction
    {
        [Test]
        public void can_translate_request_to_UrlFormat_no_data_no_version_leading_2_chars()
        {
            var url = "~/test/url/info.json";
            var req = new BatchRequest(url, 0);
            Assert.AreEqual(req.ToString(),"_REQUEST_0_URL=~/api.ashx/v2/test/url/info.json&_REQUEST_0_METHOD=GET");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_no_data_no_version_leading_1_chars()
        {
            var url = "/test/url/info.json";
            var req = new BatchRequest(url, 0);
            Assert.AreEqual(req.ToString(), "_REQUEST_0_URL=~/api.ashx/v2/test/url/info.json&_REQUEST_0_METHOD=GET");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_no_data_no_version()
        {
            var url = "test/url/info.json";
            var req = new BatchRequest(url, 0);
            Assert.AreEqual(req.ToString(), "_REQUEST_0_URL=~/api.ashx/v2/test/url/info.json&_REQUEST_0_METHOD=GET");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_no_data_with_version()
        {
            var url = "test/url/info.json";
            var req = new BatchRequest(url, 0){ApiVersion =3};
            Assert.AreEqual(req.ToString(), "_REQUEST_0_URL=~/api.ashx/v3/test/url/info.json&_REQUEST_0_METHOD=GET");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_no_data_upper_sequence()
        {
            var url = "test/url/info.json";
            var req = new BatchRequest(url, 1);
            Assert.AreEqual(req.ToString(), "_REQUEST_1_URL=~/api.ashx/v2/test/url/info.json&_REQUEST_1_METHOD=GET");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_with_data_no_version()
        {
            var url = "test/url/info.json";
            var req = new BatchRequest(url, 0);
            req.RequestParameters.Add("parm1","parm-data-1");
            req.RequestParameters.Add("parm2", "parm-data-2");
            Assert.AreEqual(req.ToString(), "_REQUEST_0_URL=~/api.ashx/v2/test/url/info.json&_REQUEST_0_METHOD=GET&_REQUEST_0_DATA=parm1%3dparm-data-1%26parm2%3dparm-data-2");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_with_data_with_version_sequence()
        {
            var url = "test/url/info.json";
            var req = new BatchRequest(url, 2){ApiVersion =4};
            req.RequestParameters.Add("parm1", "parm-data-1");
            req.RequestParameters.Add("parm2", "parm-data-2");
            Assert.AreEqual(req.ToString(), "_REQUEST_2_URL=~/api.ashx/v4/test/url/info.json&_REQUEST_2_METHOD=GET&_REQUEST_2_DATA=parm1%3dparm-data-1%26parm2%3dparm-data-2");

        }
        [Test]
        public void can_translate_request_to_UrlFormat_with_single_data_with_version_sequence()
        {
            var url = "test/url/info.json";
            var req = new BatchRequest(url, 2) { ApiVersion = 4 };
            req.RequestParameters.Add("parm1", "parm-data-1");
            Assert.AreEqual(req.ToString(), "_REQUEST_2_URL=~/api.ashx/v4/test/url/info.json&_REQUEST_2_METHOD=GET&_REQUEST_2_DATA=parm1%3dparm-data-1");

        }
    }
}
