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
    
        Task<XElement> GetEndpointXml(RestHost host,int version, string endpoint, bool enableImpersonation=true,RestGetOptions options = null);
        Task< XElement> PutEndpointXml(RestHost host, int version, string endpoint, bool enableImpersonation = true,RestPutOptions options= null);
        Task<XElement> PostEndpointXml(RestHost host, int version, string endpoint,  HttpPostedFileBase file=null, bool enableImpersonation=true,RestPostOptions options = null);
        Task<XElement> DeleteEndpointXml(RestHost host, int version, string endpoint,bool enableImpersonation =true,RestDeleteOptions options = null);

		Task<System.IO.Stream> PostEndpointStream(RestHost host,int version, string endpoint, System.IO.Stream postStream, bool enableImpersonation, Action<System.Net.WebResponse> responseAction=null,RestPostOptions options = null);

        string FormatDateTime(DateTime date);

        Task<string> GetEndpointString(RestHost host, int version, string endpoint,RestGetOptions options = null);
        Task<string> PutEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation = true,RestPutOptions options = null);
        Task<string> PostEndpointString(RestHost host, int version, string endpoint, bool enableImpersonation = true, HttpPostedFileBase file = null,RestPostOptions options = null);
        Task<string> DeleteEndpointString(RestHost host, int version, string endpoint,bool enableImpersonation = true,RestDeleteOptions options = null);

        Task<string> BatchEndpointString(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null);
        Task<XElement> BatchEndpointXml(RestHost host, int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null);
        string ReplaceTokens(string url,NameValueCollection parameters);
        string BuildQueryString(string url, NameValueCollection nvc);
    }
}
