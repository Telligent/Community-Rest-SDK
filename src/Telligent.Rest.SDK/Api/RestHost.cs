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
//using Telligent.Common.Security;
using Telligent.Evolution.RestSDK.Json;
// Telligent.Evolution.RestSDK.Services;
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

		public abstract void ApplyAuthenticationToHostRequest(System.Net.HttpWebRequest request, bool forAccessingUser);

	

		public virtual bool RetryFailedRemoteRequest(System.Net.HttpWebRequest failedRequest)
		{
			return false;
		}

        public abstract string EvolutionRootUrl { get; }
        public virtual int GetTimeout { get { return 90000; } }
        public virtual int PostTimeout { get { return 90000; } }

        #endregion

		

        public virtual HttpContextBase GetCurrentHttpContext()
        {
            var context = HttpContext.Current;
            if (context == null)
                return null;

            return new HttpContextWrapper(context);
        }

        #region Cache

        private  IRestCache _cache = null;
        public virtual IRestCache Cache
        {
            get
            {
                if (_cache == null)
                    _cache = new Telligent.Evolution.RestSDK.Implementations.SimpleCache();

                return _cache;
            }
        }
        public virtual void ExpireCaches()
        {
            Cache.Clear();
           
        }

		public System.Collections.Hashtable Items
		{
			get { return _items; }
		}

        #endregion

        #region Error Logging

        public virtual void LogError(string message, Exception ex)
        {
        }

        #endregion
      
		#region REST


        public Task<XElement> GetToXmlAsync(int version,string endpoint, bool enableImpersonation,RestGetOptions options = null)
        {
            return Rest.GetEndpointXmlAsync(this, version,endpoint, enableImpersonation,options);
        }

        public Task<XElement> PutToXmlAsync(int version, string endpoint, bool enableImpersonation = true,RestPutOptions options = null)
        {
            return Rest.PutEndpointXmlAsync(this, version, endpoint, enableImpersonation, options);
        }



        public Task<XElement> PostToXmlAsync(int version, string endpoint, HttpPostedFileBase file, bool enableImpersonation,RestPostOptions options = null)
        {
            return Rest.PostEndpointXmlAsync(this, version, endpoint, file, enableImpersonation, options);
        }

        public Task<XElement> DeleteToXmlAsync(int version, string endpoint,bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointXmlAsync(this, version, endpoint, enableImpersonation, options);
        }

        public Task<XElement> BatchRequestToXmlAsync(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointXmlAsync(this, version, requests, enableImpersonation, options);
        }

        public async Task<dynamic> GetToDynamicAsync(int version, string endpoint,bool enableImpersonation =true,RestGetOptions options = null)
        {
            var json = await Rest.GetEndpointStringAsync(this, version, endpoint,enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> PutToDynamicAsync(int version, string endpoint,bool enableImpersonation = true,RestPutOptions options = null)
        {
            var json = await Rest.PutEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> PostToDynamicAsync(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            var json = await Rest.PostEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> DeleteToDynamicAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var json = await Rest.DeleteEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> BatchRequestToDynamicAsync(int version,IList<BatchRequest> requests , bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var json = await Rest.BatchEndpointStringAsync(this, version,  requests, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public XElement GetToXml(int version, string endpoint, bool enableImpersonation, RestGetOptions options = null)
        {
            return Rest.GetEndpointXml( this, version, endpoint, enableImpersonation, options);
        }

        public XElement PutToXml(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return Rest.PutEndpointXml(this, version, endpoint, enableImpersonation, options);
        }
        public XElement BatchRequestToXml(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointXml(this, version, requests, enableImpersonation, options);
        }


        public XElement PostToXml(int version, string endpoint, HttpPostedFileBase file, bool enableImpersonation, RestPostOptions options = null)
        {
            return Rest.PostEndpointXml( this, version, endpoint, file, enableImpersonation, options);
        }

        public XElement DeleteToXml(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointXml(this, version, endpoint, enableImpersonation, options);
        }

        public string FormatRestDateTime(DateTime date)
        {
            return Rest.FormatDateTime(date);
        }

        public dynamic GetToDynamic(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            var json =  Rest.GetEndpointString(this, version, endpoint,enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public dynamic PutToDynamic(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            var json =  Rest.PutEndpointString(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public dynamic PostToDynamic(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            var json =  Rest.PostEndpointString(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        public dynamic BatchRequestToDynamic(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            var json =  Rest.BatchEndpointString(this, version, requests, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }
        public dynamic DeleteToDynamic(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var json = Rest.DeleteEndpointString(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }


        public string GetToString(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return Rest.GetEndpointString(this,version, endpoint, enableImpersonation, options);
        }

        public string PutToString(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return Rest.PutEndpointString( this,version, endpoint, enableImpersonation, options);

        }

        public string PostToString(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return Rest.PostEndpointString( this,version, endpoint, enableImpersonation, options);
        }
        public string BatchRequestToString(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointString( this,version, requests, enableImpersonation, options);
        }
        public string DeleteToString(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointString(this,version, endpoint, enableImpersonation, options);
        }


        public async Task<string> GetToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return await Rest.GetEndpointStringAsync(this, version, endpoint, enableImpersonation,options);

        }

        public async Task<string> PutToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return await Rest.PutEndpointStringAsync(this, version, endpoint, enableImpersonation, options);
        }

        public async Task<string> PostToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return await Rest.PostEndpointStringAsync(this, version, endpoint, enableImpersonation, options);

        }

        public async Task<string> DeleteToStringAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return await Rest.DeleteEndpointStringAsync(this, version, endpoint, enableImpersonation, options);

        }

        public async Task<string> BatchRequestToStringAsync(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
           return await Rest.BatchEndpointStringAsync(this, version, requests, enableImpersonation, options);
            
        }


        public Stream GetToStream(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return Rest.GetEndpointStream(this, version, endpoint, enableImpersonation, options);
        }

        public Stream PutToStream(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return Rest.PutEndpointStream(this, version, endpoint, enableImpersonation, options);

        }
        public  Task<Stream> BatchRequestToStreamAsync(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return  Rest.BatchEndpointStreamAsync(this, version, requests, enableImpersonation, options);

        }
        public Stream PostToStream(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return Rest.PostEndpointStream(this, version, endpoint, enableImpersonation, options);
        }
        public Stream BatchRequestToStream(int version, IList<BatchRequest> requests, bool enableImpersonation = true, BatchRequestOptions options = null)
        {
            return Rest.BatchEndpointStream(this, version, requests, enableImpersonation, options);
        }
        public Stream DeleteToStream(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointStream(this, version, endpoint, enableImpersonation, options);
        }


        public  Task<Stream> GetToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return   Rest.GetEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);

        }

        public Task<Stream> PutToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return  Rest.PutEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);
        }

        public  Task<Stream> PostToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return  Rest.PostEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);

        }

        public  Task<Stream> DeleteToStreamAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return  Rest.DeleteEndpointStreamAsync(this, version, endpoint, enableImpersonation, options);

        }

        public UploadedFileInfo UploadFile(UploadedFile file, RestFileOptions options = null)
        {
            return Rest.TransmitFile(this, file, options);
        }
        public Task<UploadedFileInfo> UploadFileAsync(UploadedFile file, RestFileOptions options = null)
        {
            return Rest.TransmitFileAsync(this, file, options);
        }
        #endregion
    }
}
