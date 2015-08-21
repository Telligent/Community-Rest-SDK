using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.RestSDK.IntegrationTests.ClientCredentials
{
    [TestFixture]
    public class get_tests
    {
        [Test]
        public async Task can_do_get_single_dynamic_request()
        {
            var host = new ClientCredentialsRestHost();
            var endpoint = "/users/2100.json";
            dynamic info = await host.GetToDynamicAsync(2, endpoint);

            Assert.IsNotNull(info.User.Username);
        }
        [Test]
        public async Task can_do_get_single_dynamic_request_tokenized()
        {
            var endpoint = "users/{userid}.json";
            RestGetOptions o = new RestGetOptions();
            o.PathParameters.Add("userid", "2100");
            var host = new ClientCredentialsRestHost();
            dynamic info = await host.GetToDynamicAsync(2, endpoint, true, o);

            Assert.IsNotNull(info.User.Username);
        }
        [Test]
        public void can_do_get_single_synchronous_dynamic_request()
        {
            var endpoint = "info.json";
            var host = new ClientCredentialsRestHost();
            dynamic info = host.GetToDynamic(2, endpoint);

            Assert.IsNotNull(info.InfoResult.SiteName);
        }
        [Test]
        public void can_do_get_multiple_dynamic_request()
        {
            var endpoint = "info.json";
            var list = new List<Task<dynamic>>();
            var host = new ClientCredentialsRestHost();
            list.Add(host.GetToDynamicAsync(2, endpoint));
            list.Add(host.GetToDynamicAsync(2, endpoint));
            list.Add(host.GetToDynamicAsync(2, endpoint));
            list.Add(host.GetToDynamicAsync(2, endpoint));
            list.Add(host.GetToDynamicAsync(2, endpoint));


            dynamic o1 = host.GetToDynamicAsync(2, endpoint);
            dynamic o2 = host.GetToDynamicAsync(2, endpoint);
            dynamic o3 = host.GetToDynamicAsync(2, endpoint);
            dynamic o4 = host.GetToDynamicAsync(2, endpoint);
            dynamic o5 = host.GetToDynamicAsync(2, endpoint);
            Task.WaitAll(list.ToArray());

            foreach (var o in list)
            {
                Assert.IsNotNull(o.Result.InfoResult.SiteName);
            }
        }

        [Test]
        public void can_do_get_paged_request_synchronously()
        {
            var host = new ClientCredentialsRestHost();
            var options = new NameValueCollection();
            options.Add("PageSize", "50");
            options.Add("PageIndex", "0");
            options.Add("SortBy", "LastPost");
            options.Add("SortOrder", "Descending");
            RestGetOptions o = new RestGetOptions();
            o.QueryStringParameters = options;
            var endpoint = "forums.json";


            var response = host.GetToDynamic(2, endpoint, true, o);
            Assert.IsNotNull(response.Forums);
            Assert.AreEqual(50, response.PageSize);
        }
        [Test]
        public async Task can_do_get_paged_dynamic_request()
        {
            var options = new NameValueCollection();
            var host = new ClientCredentialsRestHost();
            options.Add("PageSize", "50");
            options.Add("PageIndex", "0");
            options.Add("SortBy", "LastPost");
            options.Add("SortOrder", "Descending");
            RestGetOptions o = new RestGetOptions();
            o.QueryStringParameters = options;
            var endpoint = "forums.json";


            var response = await host.GetToDynamicAsync(2, endpoint, true, o);
            Assert.IsNotNull(response.Forums);
            Assert.AreEqual(50, response.PageSize);
        }

        [Test]
        public void can_do_impersonated_request()
        {
            //This test is environment specific
            var endpoint = "info.json";
            var host = new ClientCredentialsRestHost();
            dynamic info = null;
            host.Impersonate("pmason", (h) =>
            {
                info =  host.GetToDynamic(2, endpoint);
            });
       

            Assert.IsNotNull(info.InfoResult.SiteName);
            Assert.AreEqual("pmason",info.AccessingUser.Username.ToString());
        }

      

      
    }
}