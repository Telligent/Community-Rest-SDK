using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;

namespace Telligent.RestSDK.IntegrationTests
{
    public class get_tests:IntegrationTestBase
    {
        [Test]
        public async Task can_do_get_single_dynamic_request()
        {
            var endpoint = "/users/2100.json";
            dynamic info = await Host.GetToDynamicAsync(2,endpoint);

            Assert.IsNotNull(info);
        }
        [Test]
        public void  can_do_get_single_synchronous_dynamic_request()
        {
            var endpoint = "/info.json";
            dynamic info =  Host.GetToDynamic(2, endpoint);

            Assert.IsNotNull(info.InfoResult.SiteName);
        }
        [Test]
        public async Task can_do_get_multiple_dynamic_request()
        {
            var endpoint = "/info.json";
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
        //[Test]
        //public void can_do_get_paged_dynamic_request()
        //{
        //    var endpoint = "/users.json";
        //    dynamic users = Host.GetToDynamic(2, endpoint);

        //   
        //Assert.IsNotNull(info.InfoResult.SiteName);
        //}
        [Test]
        public void can_do_get_paged_request_synchronously()
        {

            var options = new NameValueCollection();
            options.Add("PageSize", "50");
            options.Add("PageIndex", "0");
            options.Add("SortBy", "LastPost");
            options.Add("SortOrder", "Descending");

            var endpoint = "forums.json?" +  String.Join("&",options.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(options[a])));
      

            var response = Host.GetToDynamic(2, endpoint);
            Assert.IsNotNull(response);
        }
        [Test]
        public async Task can_do_get_paged_dynamic_request()
        {
            var options = new NameValueCollection();
            options.Add("PageSize", "1");
            options.Add("PageIndex", "0");
           // options.Add("SortBy", "LastPost");
          //  options.Add("SortOrder", "Descending");

            var endpoint = "forums.json?" + String.Join("&", options.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(options[a])));


            var response = await Host.GetToDynamicAsync(2, endpoint);
            Assert.IsNotNull(response);
        }
    }
}
