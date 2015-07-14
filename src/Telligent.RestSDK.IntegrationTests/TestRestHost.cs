using System;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.RestSDK.IntegrationTests
{
    public class TestRestHost : RestHost
    {
        private static readonly Guid _id = new Guid("0b1dcd56-2639-4a82-bbd2-8ca1dd9f7ce7");
        private string _token;
        private string _url;

        public TestRestHost(string url,string token)
        {
            _token = token;
            _url = url;
        }

        public override void ApplyAuthenticationToHostRequest(System.Net.HttpWebRequest request, bool forAccessingUser)
        {
            request.Headers["Authorization"] = "OAuth " + _token;
        }

        public override string EvolutionRootUrl
        {
            get { return _url; }
        }

        public override string Name
        {
            get { return "default"; }
        }

        public override void LogError(string message, Exception ex)
        {
            Console.WriteLine(message);
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

      
    }
}
