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

		public abstract Guid Id { get; }
        public abstract string Name { get; }
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
            return Rest.GetEndpointXml(this, version,endpoint, enableImpersonation,options);
        }

        public Task<XElement> PutToXmlAsync(int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null)
        {
            return Rest.PutEndpointXml(this, version, endpoint, postData, enableImpersonation, options);
        }



        public Task<XElement> PostToXmlAsync(int version, string endpoint, string postData, HttpPostedFileBase file, bool enableImpersonation,RestPostOptions options = null)
        {
            return Rest.PostEndpointXml(this, version, endpoint, postData, file, enableImpersonation, options);
        }

        public Task<XElement> DeleteToXmlAsync(int version, string endpoint,bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointXml(this, version, endpoint, enableImpersonation, options);
        }

       

        public async Task<dynamic> GetToDynamicAsync(int version, string endpoint,bool enableImpersonation =true,RestGetOptions options = null)
        {
            var json = await Rest.GetEndpointJson(this, version, endpoint, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> PutToDynamicAsync(int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null)
        {
            var json = await Rest.PutEndpointJson(this, version, endpoint, postData, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> PostToDynamicAsync(int version, string endpoint, string postData, bool enableImpersonation = true, RestPostOptions options = null)
        {
            var json = await Rest.PostEndpointJson(this, version, endpoint, postData, enableImpersonation, null, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public async Task<dynamic> DeleteToDynamicAsync(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var json = await Rest.DeleteEndpointJson(this, version, endpoint, enableImpersonation, options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        private async Task<dynamic> Deserialize(string json)
        {
            var deserializer = ServiceLocator.Get<IDeserializer>();

            dynamic result = new ExpandoObject();

           await deserializer.Deserialize(result, new JsonReader(json));

            return result;
        }




        public XElement GetToXml(int version, string endpoint, bool enableImpersonation, RestGetOptions options = null)
        {
            return GetToXmlAsync( version, endpoint, enableImpersonation, options).Result;
        }

        public XElement PutToXml(int version, string endpoint, string postData, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return PutToXmlAsync( version, endpoint, postData, enableImpersonation, options).Result;
        }



        public XElement PostToXml(int version, string endpoint, string postData, HttpPostedFileBase file, bool enableImpersonation, RestPostOptions options = null)
        {
            return PostToXmlAsync( version, endpoint, postData, file, enableImpersonation, options).Result;
        }

        public XElement DeleteToXml(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return DeleteToXmlAsync( version, endpoint, enableImpersonation, options).Result;
        }

        public string FormatRestDateTime(DateTime date)
        {
            return Rest.FormatDateTime(date);
        }

        public dynamic GetToDynamic(int version, string endpoint, bool enableImpersonation = true, RestGetOptions options = null)
        {
            return GetToDynamicAsync(version, endpoint, enableImpersonation, options).Result;
        }

        public dynamic PutToDynamic(int version, string endpoint, string postData, bool enableImpersonation = true, RestPutOptions options = null)
        {
            return PutToDynamicAsync( version, endpoint, postData, enableImpersonation, options).Result;
            
        }

        public dynamic PostToDynamic(int version, string endpoint, string postData, bool enableImpersonation = true, RestPostOptions options = null)
        {
            return PostToDynamicAsync( version, endpoint, postData,enableImpersonation, options).Result;
        }

        public dynamic DeleteToDynamic(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return DeleteToDynamicAsync(version, endpoint,enableImpersonation,options).Result;
        }
        
        #endregion

		public static void Register(RestHost host)
		{
			ServiceLocator.Get<IRestHostRegistrationService>().Register(host);
		}

		public static void Unregister(Guid id)
		{
            ServiceLocator.Get<IRestHostRegistrationService>().Remove(id);
		}

		public static RestHost Get(Guid id)
		{
            return ServiceLocator.Get<IRestHostRegistrationService>().Get(id);
		}
    }
}
