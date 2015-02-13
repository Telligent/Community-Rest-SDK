using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;


namespace Telligent.Rest.SDK.Model
{
    public interface IRest
    {
    
        Task<XElement> GetEndpointXmlAsync(RestHost host,int version, string endpoint, bool enableImpersonation=true,RestGetOptions options = null);
        Task<XElement> PutEndpointXmlAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null);
        Task<XElement> PostEndpointXmlAsync(RestHost host, int version, string endpoint, HttpPostedFileBase file = null, bool enableImpersonation = true, RestPostOptions options = null);
        Task<XElement> DeleteEndpointXmlAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null);

        XElement GetEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null);
        XElement PutEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null);
        XElement PostEndpointXml(RestHost host, int version, string endpoint, HttpPostedFileBase file = null, bool enableImpersonation = true, RestPostOptions options = null);
        XElement DeleteEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null);

		Task<System.IO.Stream> PostEndpointStream(RestHost host,int version, string endpoint, System.IO.Stream postStream, bool enableImpersonation, Action<System.Net.WebResponse> responseAction=null,RestPostOptions options = null);

        string FormatDateTime(DateTime date);

        Task<string> GetEndpointStringAsync(RestHost host, int version, string endpoint,bool enableImpersonation =true,RestGetOptions options = null);
        Task<string> PutEndpointStringAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null);
        Task<string> PostEndpointStringAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null);
        Task<string> DeleteEndpointStringAsync(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null);

        string GetEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation =true,RestGetOptions options = null);
        string PutEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null);
        string PostEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation = true,  RestPostOptions options = null);
        string DeleteEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null);

        Task<string> BatchEndpointStringAsync(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null);
        Task<XElement> BatchEndpointXmlAsync(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null);

        string BatchEndpointString(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null);
        XElement BatchEndpointXml(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null);

        string ReplaceTokens(string url,NameValueCollection parameters);
        string BuildQueryString(string url, NameValueCollection nvc);
    }
}
