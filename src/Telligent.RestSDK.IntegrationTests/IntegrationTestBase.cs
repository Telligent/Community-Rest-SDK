using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Telligent.Evolution.Extensibility.Rest.Version1;


namespace Telligent.RestSDK.IntegrationTests
{
    [TestFixture]
    public abstract class IntegrationTestBase
    {
        

        private RestHost _host;
        public RestHost Host
        {
            get
            {
                if (_host == null)
                {
                    if(string.IsNullOrEmpty(Setup.Token))
                        throw new Exception("Failed to obtain access token in suite setup");

                    _host = new TestRestHost(Setup.CommunityUrl,Setup.Token);
                }

                

                return _host;
            }
        }
    }


}
