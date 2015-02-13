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
    public class post_tests:IntegrationTestBase
    {
        [Test]
        public async Task can_do_post_async_single_dynamic_request()
        {
            var msg = "Hello World at " + DateTime.UtcNow.ToString();

            var endpoint = "/users/{username}/statuses.json";
            var options = new RestPostOptions();
            options.PathParameters.Add("username","admin");
            options.PostParameters.Add("MessageBody",msg);
            dynamic status = await Host.PostToDynamicAsync(2, endpoint, true, options);

            Assert.IsNotNull(status.StatusMessage);
            Assert.AreEqual(msg,status.StatusMessage.Body);
        }
        [Test]
        public async Task can_do_post_async_single_string_request()
        {
            var msg = "Hello World at " + DateTime.UtcNow.ToString();

            var endpoint = "/users/{username}/statuses.json";
            var options = new RestPostOptions();
            options.PathParameters.Add("username", "admin");
            options.PostParameters.Add("MessageBody", msg);
            var status = await Host.PostToStringAsync(2, endpoint, true, options);

            Assert.IsFalse(string.IsNullOrWhiteSpace(status));
            Assert.IsTrue(status.Contains(msg));
        }
        [Test]
        public async Task can_do_post_async_single_stream_request()
        {
            var msg = "Hello World at " + DateTime.UtcNow.ToString();

            var endpoint = "/users/{username}/statuses.json";
            var options = new RestPostOptions();
            options.PathParameters.Add("username", "admin");
            options.PostParameters.Add("MessageBody", msg);
            using (var status = await Host.PostToStreamAsync(2, endpoint, true, options))
            {
                Assert.IsNotNull(status);
                Assert.Greater(status.Length, 0);
            }

        }
        [Test]
        public  void can_do_post_async_single_stream_request_sync()
        {
            var msg = "Hello World at " + DateTime.UtcNow.ToString();

            var endpoint = "/users/{username}/statuses.json";
            var options = new RestPostOptions();
            options.PathParameters.Add("username", "admin");
            options.PostParameters.Add("MessageBody", msg);
            using (var status =  Host.PostToStream(2, endpoint, true, options))
            {
                Assert.IsNotNull(status);
                Assert.Greater(status.Length, 0);
            }

        }
    }
}
