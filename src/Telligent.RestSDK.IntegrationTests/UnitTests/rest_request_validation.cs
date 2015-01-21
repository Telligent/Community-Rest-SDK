using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Services;

namespace Telligent.RestSDK.IntegrationTests.UnitTests
{
    [TestFixture]
   public  class rest_request_validation
    {
        private IRest Rest;
        private RestHost Host;
        [TestFixtureSetUp]
        public void Setup()
        {
            Rest = new Evolution.RestSDK.Implementations.Rest();
            Host = new TestRestHost(null,null);
        }
        #region JSON
        [Test]
        public void cannot_call_Get_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.GetEndpointJson(Host, 2, url, null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint",StringComparison.InvariantCultureIgnoreCase));
        }






        [Test]
        public void cannot_call_post_overload_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PostEndpointJson(Host, 2, url, null,null,null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_post_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PostEndpointJson(Host, 2, url, null, null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_put_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PutEndpointJson(Host, 2, url, null,true,null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_delete_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.DeleteEndpointJson(Host,2,url,true,null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        #endregion

        #region XML
        [Test]
        public void cannot_call_Get_Xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.GetEndpointXml(Host, 2, url, null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }


        [Test]
        public void cannot_call_post_overload_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PostEndpointXml(Host,2,url,null,null,true,null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_post_overload1_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PostEndpointXml(Host, 2, url,null,null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_post_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PostEndpointXml(Host, 2, url, null,true);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_put_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.PutEndpointXml(Host, 2, url, null, true, null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        [Test]
        public void cannot_call_delete_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                Rest.DeleteEndpointXml(Host, 2, url, true, null);
            });
            Assert.IsTrue(ex.ParamName.Equals("endpoint", StringComparison.InvariantCultureIgnoreCase));
        }
        #endregion
    }
}
