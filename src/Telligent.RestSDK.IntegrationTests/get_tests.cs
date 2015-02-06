using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.RestSDK.IntegrationTests
{
    public class get_tests:IntegrationTestBase
    {
        [Test]
        public async Task can_do_get_single_dynamic_request()
        {
            var endpoint = "/users/2100.json";
            dynamic info = await Host.GetToDynamicAsync(2,endpoint);

            Assert.IsNotNull(info.User.Username);
        }
        [Test]
        public async Task can_do_get_single_dynamic_request_tokenized()
        {
            var endpoint = "users/{userid}.json";
            RestGetOptions o = new RestGetOptions();
            o.PathParameters.Add("userid","2100");
            dynamic info = await Host.GetToDynamicAsync(2, endpoint,true,o);

            Assert.IsNotNull(info.User.Username);
        }
        [Test]
        public void  can_do_get_single_synchronous_dynamic_request()
        {
            var endpoint = "info.json";
            dynamic info =  Host.GetToDynamic(2, endpoint);

            Assert.IsNotNull(info.InfoResult.SiteName);
        }
        [Test]
        public async Task can_do_get_multiple_dynamic_request()
        {
            var endpoint = "info.json";
            var o1 =  Host.GetToDynamicAsync(2, endpoint);
            var o2 = Host.GetToDynamicAsync(2, endpoint);
            var o3 = Host.GetToDynamicAsync(2, endpoint);
            var o4 = Host.GetToDynamicAsync(2, endpoint);
            var o5 = Host.GetToDynamicAsync(2, endpoint);
            Task.WaitAll();
            Assert.IsNotNull(o1);
            Assert.IsNotNull(o2);
            Assert.IsNotNull(o3);
            Assert.IsNotNull(o4);
            Assert.IsNotNull(o5);
        }
     
        [Test]
        public void can_do_get_paged_request_synchronously()
        {

            var options = new NameValueCollection();
            options.Add("PageSize", "50");
            options.Add("PageIndex", "0");
            options.Add("SortBy", "LastPost");
            options.Add("SortOrder", "Descending");
            RestGetOptions o = new RestGetOptions();
            o.QueryStringParameters = options;
            var endpoint = "forums.json";
      

            var response = Host.GetToDynamic(2, endpoint,true,o);
            Assert.IsNotNull(response.Forums);
            Assert.AreEqual(50,response.PageSize);
        }
        [Test]
        public async Task can_do_get_paged_dynamic_request()
        {
            var options = new NameValueCollection();
            options.Add("PageSize", "50");
            options.Add("PageIndex", "0");
            options.Add("SortBy", "LastPost");
            options.Add("SortOrder", "Descending");
            RestGetOptions o = new RestGetOptions();
            o.QueryStringParameters = options;
            var endpoint = "forums.json";


            var response = await Host.GetToDynamicAsync(2, endpoint,true,o);
            Assert.IsNotNull(response.Forums);
            Assert.AreEqual(50, response.PageSize);
        }

        [Test]
        public async Task  can_do_batch_request_non_sequential_to_dynamic()
        {
            var req1 = new BatchRequest("info.json", 0) {ApiVersion = 2};
            req1.RequestParameters.Add("ShowSiteSettings","true");
            var req2 = new BatchRequest("users.json", 1);

            req2.RequestParameters.Add("PageSize","5");
            req1.RequestParameters.Add("IncludeHidden","true");

            var requests = new List<BatchRequest>() {req1, req2};
            var resp = await Host.BatchRequestToDynamicAsync(2, requests);

            Assert.IsNotNull(resp.BatchResponses);
            Assert.IsNotNull(resp.BatchResponses[0]);
            Assert.IsNotNull(resp.BatchResponses[1]);
            Assert.AreEqual(200,resp.BatchResponses[0].StatusCode);
            Assert.AreEqual(200, resp.BatchResponses[1].StatusCode);
            Assert.IsNotNull(resp.BatchResponses[0].BatchResponse.InfoResult);
            Assert.IsNotNull(resp.BatchResponses[1].BatchResponse.Users);
           
        }

        [Test]
        public async Task can_do_get_single_string_request()
        {
            var endpoint = "/users/2100.json";
            var info = await Host.GetToStringAsync(2, endpoint);

           Assert.IsFalse(string.IsNullOrWhiteSpace(info));
            Assert.IsTrue(info.Contains("admin"));
        }
    }
}
