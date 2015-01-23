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
        Task<string> Post(RestHost host, string url, string data, HttpPostedFileBase file, Action<HttpWebRequest> adjustRequest);
        Task<string> Get(RestHost host, string url, Action<HttpWebRequest> adjustRequest);
    }
}