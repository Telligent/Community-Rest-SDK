using System;
using System.Collections.Generic;
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
        Task< XElement> PutEndpointXml(RestHost host, int version, string endpoint, string postData, bool enableImpersonation = true,RestPutOptions options= null);
        Task<XElement> PostEndpointXml(RestHost host, int version, string endpoint, string postData, HttpPostedFileBase file=null, bool enableImpersonation=true,RestPostOptions options = null);
        Task<XElement> DeleteEndpointXml(RestHost host, int version, string endpoint,bool enableImpersonation =true,RestDeleteOptions options = null);

		Task<System.IO.Stream> PostEndpointStream(RestHost host,int version, string endpoint, System.IO.Stream postStream, bool enableImpersonation, Action<System.Net.WebResponse> responseAction=null,RestPostOptions options = null);

        string FormatDateTime(DateTime date);

        Task<string> GetEndpointJson(RestHost host, int version, string endpoint,RestGetOptions options = null);
        Task<string> PutEndpointJson(RestHost host, int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null);
        Task<string> PostEndpointJson(RestHost host, int version, string endpoint, string postData,bool enableImpersonation = true, HttpPostedFileBase file = null,RestPostOptions options = null);
        Task<string> DeleteEndpointJson(RestHost host, int version, string endpoint,bool enableImpersonation = true,RestDeleteOptions options = null);

    }
}
