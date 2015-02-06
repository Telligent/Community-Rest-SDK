using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;
using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK.Model;

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
            Rest = new Evolution.RestSDK.Implementations.Rest(new Mock<IRestCommunicationProxy>().Object);
            Host = new TestRestHost(null);
        }
        #region JSON
        [Test]
        public async Task cannot_call_Get_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            Assert.That(async () => await Rest.GetEndpointJson(Host,2,url,null),Throws.ArgumentException);
        }

        [Test]
        public async Task cannot_call_post_overload_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            Assert.That(async () => await   Rest.PostEndpointJson(Host, 2, url,false,null), Throws.ArgumentException);
        }
        [Test]
        public async Task cannot_call_post_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            Assert.That(async () => await Rest.PostEndpointJson(Host, 2, url, false), Throws.ArgumentException);
            
        }
        [Test]
        public async Task cannot_call_put_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";
            Assert.That(async () => await  Rest.PutEndpointJson(Host, 2, url,true,null), Throws.ArgumentException);
        }
        [Test]
        public async Task cannot_call_delete_json_endpoint_with_xml_extension()
        {
            var url = "/info.xml";

            Assert.That(async () => await Rest.DeleteEndpointJson(Host, 2, url, true, null), Throws.ArgumentException);

        }
        #endregion

        #region XML
        [Test]
        public async Task cannot_call_Get_Xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            Assert.That(async () => await Rest.GetEndpointXml(Host, 2, url, false), Throws.ArgumentException);
        }


     
        [Test]
        public async Task cannot_call_post_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";

            Assert.That(async () => await Rest.PostEndpointXml(Host, 2, url, null, true), Throws.ArgumentException);
        }
        [Test]
        public async Task cannot_call_put_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            Assert.That(async () => await Rest.PutEndpointXml(Host, 2, url, true, null), Throws.ArgumentException);
        }
        [Test]
        public async Task cannot_call_delete_xml_endpoint_with_json_extension()
        {
            var url = "/info.json";
            Assert.That(async () => await Rest.DeleteEndpointXml(Host, 2, url, true, null), Throws.ArgumentException);
 
        }
        #endregion
    }
}
