using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Telligent.Rest.SDK;

namespace Telligent.RestSDK.UnitTests.Urls
{
    [TestFixture]
    public class when_parsing_callback_url
    {
        private static IUrlManipulationService _urls;

        [TestFixtureSetUp]
        public void Setup()
        {
            _urls = new UrlManipulationService(new Encode(),new Decode());
        }

        [Test]
        public void can_parse_hostName_From_Url()
        {
            var url =
                _urls.GetCallbackUrl(null, "~/callback.ashx", "somedefaulthostname",
                    "~/community.telligent.com/somegroup/group/b/myblog/mypost", "");

            var data = _urls.ParseCallbackUrl(url);
            Assert.AreEqual(data.HostName, "somedefaulthostname");
           
        }
        [Test]
        public void can_parse_hostNameWithspaces_From_Url()
        {
            var url =
                _urls.GetCallbackUrl(null, "~/callback.ashx", "some default host name",
                    "~/community.telligent.com/somegroup/group/b/myblog/mypost", "");

            var data = _urls.ParseCallbackUrl(url);
            Assert.AreEqual(data.HostName, "some default host name");

        }
        [Test]
        public void can_parse_remoteUrl_From_Url()
        {
            var url =
                _urls.GetCallbackUrl(null, "~/callback.ashx", "somedefaulthostname",
                    "~/community.telligent.com/somegroup/group/b/myblog/mypost", "");

            var data = _urls.ParseCallbackUrl(url);
            Assert.AreEqual(data.Url, "~/community.telligent.com/somegroup/group/b/myblog/mypost");

        }

        [Test]
        public void can_encode_hostname_with_spaces()
        {
            var url =
               _urls.GetCallbackUrl(null, "~/callback.ashx", "some default host name",
                   "~/community.telligent.com/somegroup/group/b/myblog/mypost", "");

            Assert.AreEqual(url,"~/callback.ashx/rhn_some+default+host+name/~/community.telligent.com/somegroup/group/b/myblog/mypost");
        }
        [Test]
        public void can_create_callback_with_Querystring()
        {
            var url =
               _urls.GetCallbackUrl(null, "~/callback.ashx", "defaulthost",
                   "~/community.telligent.com/somegroup/group/b/myblog/mypost", "PageIndex=0&PageSize=100");

            Assert.AreEqual(url, "~/callback.ashx/rhn_defaulthost/~/community.telligent.com/somegroup/group/b/myblog/mypost?PageIndex=0&PageSize=100");
        }
        [Test]
        public void can_parse_url_with_Querystring()
        {
            var url =
               _urls.GetCallbackUrl(null, "~/callback.ashx", "defaulthost",
                   "~/community.telligent.com/somegroup/group/b/myblog/mypost", "PageIndex=0&PageSize=100");

            var data = _urls.ParseCallbackUrl(url);
            Assert.AreEqual(data.Url, "~/community.telligent.com/somegroup/group/b/myblog/mypost?PageIndex=0&PageSize=100");
        }
    }
}
