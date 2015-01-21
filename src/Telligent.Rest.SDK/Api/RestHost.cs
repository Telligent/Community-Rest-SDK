using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

		public XElement GetRestEndpointXml(int version,string endpoint,RestGetOptions options = null)
        {
            return Rest.GetEndpointXml(this,version, endpoint,options);
        }

        public XElement GetRestEndpointXml(int version,string endpoint, bool enableImpersonation,RestGetOptions options = null)
        {
            return Rest.GetEndpointXml(this, version,endpoint, enableImpersonation,options);
        }

        public XElement PutRestEndpointXml(int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null)
        {
            return Rest.PutEndpointXml(this, version, endpoint, postData,enableImpersonation,options);
        }

        public XElement PostRestEndpointXml(int version, string endpoint, string postData,RestPostOptions options = null)
        {
            return Rest.PostEndpointXml(this, version, endpoint, postData,options);
        }

        public XElement PostRestEndpointXml(int version, string endpoint, string postData, bool enableImpersonation,RestPostOptions options = null)
        {
            return Rest.PostEndpointXml(this, version, endpoint, postData, enableImpersonation,options);
        }

        public XElement PostRestEndpointXml(int version, string endpoint, string postData, HttpPostedFileBase file, bool enableImpersonation,RestPostOptions options = null)
        {
            return Rest.PostEndpointXml(this, version, endpoint, postData, file, enableImpersonation,options);
        }

        public XElement DeleteRestEndpointXml(int version, string endpoint,bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            return Rest.DeleteEndpointXml(this, version, endpoint,enableImpersonation,options);
        }

        public string FormatRestDateTime(DateTime date)
        {
            return Rest.FormatDateTime(date);
        }

        public dynamic GetRestEndpoint(int version, string endpoint,RestGetOptions options = null)
        {
            var json = Rest.GetEndpointJson(this, version, endpoint,options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public dynamic PutRestEndpoint(int version, string endpoint, string postData,bool enableImpersonation = true,RestPutOptions options = null)
        {
            var json = Rest.PutEndpointJson(this, version, endpoint, postData,enableImpersonation,options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public dynamic PostRestEndpoint(int version, string endpoint, string postData,RestPostOptions options = null)
        {
            var json = Rest.PostEndpointJson(this, version, endpoint, postData,options);
            return json != null ? JsonConvert.Deserialize(json) : null;
        }

        public dynamic DeleteRestEndpoint(int version, string endpoint, bool enableImpersonation = true, RestDeleteOptions options = null)
        {
            var json = Rest.DeleteEndpointJson(this, version, endpoint,enableImpersonation,options);
            return json != null ? JsonConvert.Deserialize(json) : null;
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

   


