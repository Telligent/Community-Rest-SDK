using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Xml;
using System.Xml.Linq;
using Telligent.Evolution.RestSDK.Json;
using Telligent.Evolution.RestSDK.Services;
using Telligent.Rest.SDK.Model;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public abstract class RestHost
    {
      
       
        private readonly IRest Rest;
      
		private System.Collections.Hashtable _items = new System.Collections.Hashtable();

        public RestHost()
        {
            Rest = ServiceLocator.Get<IRest>();
        }


        #region Remote Authentication
        /// <summary>
        /// Used to apply authentication headers to a REST requests.  Called for each request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="forAccessingUser"></param>
		public abstract void ApplyAuthenticationToHostRequest(System.Net.HttpWebRequest request, bool forAccessingUser);

	
        /// <summary>
        /// OBSOLETE: Will be removed in a future version
        /// </summary>
        /// <param name="failedRequest"></param>
        /// <returns></returns>
        
		public virtual bool RetryFailedRemoteRequest(System.Net.HttpWebRequest failedRequest)
		{
			return false;
		}
        /// <summary>
        /// The root Url of the community
        /// </summary>
        public abstract string EvolutionRootUrl { get; }
        /// <summary>
        /// The timeout applied to GET requests
        /// </summary>
        public virtual int GetTimeout { get { return 90000; } }
        /// <summary>
        /// The timeout applied to POST,PUT and DELETE requests
        /// </summary>
        public virtual int PostTimeout { get { return 90000; } }

        #endregion

		
        /// <summary>
        /// Returns the current HttpContextBase object
        /// </summary>
        /// <returns></returns>
        public virtual HttpContextBase GetCurrentHttpContext()
        {
            var context = HttpContext.Current;
            if (context == null)
                return null;

            return new HttpContextWrapper(context);
        }

        #region Cache

        private  IRestCache _cache = null;
        /// <summary>
        /// Locally implemented cache for system objects
        /// </summary>
        public virtual IRestCache Cache
        {
            get
            {
                if (_cache == null)
                    _cache = new Telligent.Evolution.RestSDK.Implementations.SimpleCache();

                return _cache;
            }
        }
        /// <summary>
        /// Clears local cache
        /// </summary>
        public virtual void ExpireCaches()
        {
            Cache.Clear();
           
        }
        /// <summary>
        /// Can be used to store data for the lifetime of the host object
        /// </summary>
		public System.Collections.Hashtable Items
		{
			get { return _items; }
		}

        #endregion

        #region Error Logging
        /// <summary>
        /// Called when an error ocurrs
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public virtual void LogError(string message, Exception ex)
        {
        }

        #endregion
      
		#region REST

        /// <summary>
        /// REST GET Request(Async) for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        /// 
        public Task<XElement> GetToXmlAsync(int version,string endpoint, bool enableImpersonation,RestGetOptions options = null)
        {
            return Rest.GetEndpointXmlAsync(this, version,endpoint, enableImpersonation,options);
        }
        /// <summary>
        /// REST PUT Request(Async) for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        public Task<XElement> PutToXmlAsync(int version, string endpoint, bool enableImpersonation = true,RestPutOptions options = null)
        {
            return Rest.PutEndpointXmlAsync(this, version, endpoint, enableImpersonation, options);
        }

        /// <summary>
        /// REST POST Request(Async) for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>

        public Task<XElement> PostToXmlAsync(int version, string endpoint, HttpPostedFileBase file, bool enableImpersonation,RestPostOptions options = null)
        {
            return Rest.PostEndpointXmlAsync(this, version, endpoint, file, enableImpersonation, options);
        }
        /// <summary>
        /// REST DELETE Request(Async) for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        public Task<XElement> DeleteToXmlAsync(int version, string endpoint,bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointXmlAsync(this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST Batch Request for xml (Async)
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>XElement</returns>
        public Task<XElement> BatchRequestToXmlAsync(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointXmlAsync(this, version, requests, enableImpersonation, options);
        }
        /// <summary>
        /// REST GET Request(Async) for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public async Task<dynamic> GetToDynamicAsync(int version, string endpoint,bool enableImpersonation =true,RestGetOptions options = null)
        {
            var json = await Rest.GetEndpointStringAsync(this, version, endpoint,enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST PUT Request(Async) for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public async Task<dynamic> PutToDynamicAsync(int version, string endpoint,bool enableImpersonation = true,RestPutOptions options = null)
        {
            var json = await Rest.PutEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST POST Request(Async) for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public async Task<dynamic> PostToDynamicAsync(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            var json = await Rest.PostEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST DELETE Request(Async) for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public async Task<dynamic> DeleteToDynamicAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var json = await Rest.DeleteEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST Batch Request for JSON (Async)
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>dynamic</returns>
        public async Task<dynamic> BatchRequestToDynamicAsync(int version,IList<BatchRequest> requests , bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var json = await Rest.BatchEndpointStringAsync(this, version,  requests, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST GET Request for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        /// 
        public XElement GetToXml(int version, string endpoint, bool enableImpersonation, RestGetOptions options = null)
        {
            return Rest.GetEndpointXml( this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST PUT Request for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        /// 
        public XElement PutToXml(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return Rest.PutEndpointXml(this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST Batch Request for JSON 
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>XElement</returns>
        public XElement BatchRequestToXml(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointXml(this, version, requests, enableImpersonation, options);
        }

        /// <summary>
        /// REST POST Request for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        ///
        public XElement PostToXml(int version, string endpoint, HttpPostedFileBase file, bool enableImpersonation, RestPostOptions options = null)
        {
            return Rest.PostEndpointXml( this, version, endpoint, file, enableImpersonation, options);
        }
        /// <summary>
        /// REST DELETE Request for Xml
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>XElement</returns>
        ///
        public XElement DeleteToXml(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointXml(this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST GET Request for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public dynamic GetToDynamic(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            var json =  Rest.GetEndpointString(this, version, endpoint,enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST PUT Request for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public dynamic PutToDynamic(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            var json =  Rest.PutEndpointString(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// POST GET Request for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public dynamic PostToDynamic(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            var json =  Rest.PostEndpointString(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        /// <summary>
        /// REST Batch Request for JSON 
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>dynamic</returns>
        public dynamic BatchRequestToDynamic(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var json =  Rest.BatchEndpointString(this, version, requests, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        /// <summary>
        /// REST DELETE Request for JSON
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public dynamic DeleteToDynamic(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var json = Rest.DeleteEndpointString(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        /// <summary>
        /// REST GET Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>dynamic</returns>
        public string GetToString(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return Rest.GetEndpointString(this,version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST PUT Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public string PutToString(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return Rest.PutEndpointString( this,version, endpoint, enableImpersonation, options);

        }
        /// <summary>
        /// REST POST Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public string PostToString(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return Rest.PostEndpointString( this,version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST Batch Request
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>string</returns>
        public string BatchRequestToString(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointString( this,version, requests, enableImpersonation, options);
        }
        /// <summary>
        /// REST DELETE Request(Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public string DeleteToString(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointString(this,version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST GET Request(Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public async Task<string> GetToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return await Rest.GetEndpointStringAsync(this, version, endpoint, enableImpersonation,options);

        }
        /// <summary>
        /// REST PUT Request(Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public async Task<string> PutToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return await Rest.PutEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST POST Request(Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public async Task<string> PostToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return await Rest.PostEndpointStringAsync(this, version, endpoint, enableImpersonation, options);

        }
        /// <summary>
        /// REST DELETE Request(Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>string</returns>
        public async Task<string> DeleteToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return await Rest.DeleteEndpointStringAsync(this, version, endpoint, enableImpersonation, options);

        }
        /// <summary>
        /// REST Batch Request(Async)
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>string</returns>
        public async Task<string> BatchRequestToStringAsync(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
           return await Rest.BatchEndpointStringAsync(this, version, requests, enableImpersonation, options);
            
        }

        /// <summary>
        /// REST GET Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public Stream GetToStream(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return Rest.GetEndpointStream(this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST PUT Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public Stream PutToStream(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return Rest.PutEndpointStream(this, version, endpoint, enableImpersonation, options);

        }
       
        /// <summary>
        /// REST POST Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public Stream PostToStream(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return Rest.PostEndpointStream(this, version, endpoint, enableImpersonation, options);
        }
        /// <summary>
        /// REST Batch Request
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>Stream</returns>
        public Stream BatchRequestToStream(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointStream(this, version, requests, enableImpersonation, options);
        }
        /// <summary>
        /// REST DELETE Request
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public Stream DeleteToStream(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointStream(this, version, endpoint, enableImpersonation, options);
        }

        /// <summary>
        /// REST GET Request (Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public  Task<Stream> GetToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return   Rest.GetEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);

        }

        /// <summary>
        /// REST PUT Request (Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public Task<Stream> PutToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return  Rest.PutEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);
        }

        /// <summary>
        /// REST POST Request (Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public  Task<Stream> PostToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return  Rest.PostEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);

        }

        /// <summary>
        /// REST DELETE Request (Async)
        /// </summary>
        /// <param name="version">The REST Api version</param>
        /// <param name="endpoint">The Url without aspi.ashx and ther version</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional options for this request type.</param>
        /// <returns>Stream</returns>
        public  Task<Stream> DeleteToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return  Rest.DeleteEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);

        }
        /// <summary>
        /// REST Batch Request(Async)
        /// </summary>
        /// <param name="version">The REST Api Version</param>
        /// <param name="requests">A list of BatchRequest options to execute</param>
        /// <param name="enableImpersonation">Use the locally authenticated user versus the default</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>Stream</returns>
        public Task<Stream> BatchRequestToStreamAsync(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointStreamAsync(this, version, requests, enableImpersonation, options);

        }
        /// <summary>
        /// Uploads a file to your community site for use in later requests such as attachments
        /// </summary>
        /// <param name="file">A file defintition object</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>UploadedFileInfo</returns>
        public UploadedFileInfo UploadFile(UploadedFile file, RestFileOptions options = null)
        {
            return Rest.TransmitFile(this, file, options);
        }
        /// <summary>
        /// Uploads a file to your community site for use in later requests such as attachments(Async)
        /// </summary>
        /// <param name="file">A file defintition object</param>
        /// <param name="options">Additional optional items for this request type</param>
        /// <returns>UploadedFileInfo</returns>
        public Task<UploadedFileInfo> UploadFileAsync(UploadedFile file, RestFileOptions options = null)
        {
            return Rest.TransmitFileAsync(this, file, options);
        }
        #endregion

        #region Helpers

        public string FormatRestDateTime(DateTime date)
        {
            return Rest.FormatDateTime(date);
        }

        #endregion
    }
}
