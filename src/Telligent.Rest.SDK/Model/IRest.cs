using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Web;
using Telligent.Evolution.Extensibility.Rest.Version1;


namespace Telligent.Evolution.RestSDK.Services
{
    public interface IRest
    {
        XElement GetEndpointXml(RestHost host, int version, string endpoint,RestGetOptions options = null);
        XElement GetEndpointXml(RestHost host,int version, string endpoint, bool enableImpersonation,RestGetOptions options = null);
        XElement PutEndpointXml(RestHost host, int version, string endpoint, string postData, bool enableImpersonation = true,RestPutOptions options= null);
        XElement PostEndpointXml(RestHost host, int version, string endpoint, string postData, RestPostOptions options = null);
        XElement PostEndpointXml(RestHost host, int version, string endpoint, string postData, bool enableImpersonation,RestPostOptions options = null);
        XElement PostEndpointXml(RestHost host, int version, string endpoint, string postData, HttpPostedFileBase file, bool enableImpersonation,RestPostOptions options = null);
        XElement DeleteEndpointXml(RestHost host, int version, string endpoint,bool enableImpersonation =false,RestDeleteOptions options = null);

        System.IO.Stream PostEndpointStream(RestHost host, int version, string endpoint, System.IO.Stream postStream,RestPostOptions options = null);
		System.IO.Stream PostEndpointStream(RestHost host,int version, string endpoint, System.IO.Stream postStream, bool enableImpersonation, Action<System.Net.WebResponse> responseAction,RestPostOptions options = null);

        string FormatDateTime(DateTime date);

        string GetEndpointJson(RestHost host, int version, string endpoint,RestGetOptions options = null);
        string PutEndpointJson(RestHost host, int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null);
        string PostEndpointJson(RestHost host, int version, string endpoint, string postData, RestPostOptions options = null);
        string PostEndpointJson(RestHost host, int version, string endpoint, string postData, HttpPostedFileBase file,RestPostOptions options = null);
        string DeleteEndpointJson(RestHost host, int version, string endpoint,bool enableImpersonation = true,RestDeleteOptions options = null);

    }
}
