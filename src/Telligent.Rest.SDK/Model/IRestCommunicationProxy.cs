using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;

namespace Telligent.Rest.SDK.Model
{
    public interface IRestCommunicationProxy
    {
        Task<Stream> PostEndpointStream(RestHost host, string url, Stream postStream, Action<HttpWebRequest> adjustRequest, Action<WebResponse> responseAction);
        Stream Post(RestHost host, string url, string data,  Action<HttpWebRequest> adjustRequest);
        Task<Stream> PostAsync(RestHost host, string url, string data,  Action<HttpWebRequest> adjustRequest);
        Task<Stream> GetAsync(RestHost host, string url, Action<HttpWebRequest> adjustRequest);
        Stream Get(RestHost host, string url, Action<HttpWebRequest> adjustRequest);


    }
}