using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telligent.Rest.SDK;

namespace Telligent.Rest.SDK
{
    public abstract class RestOptions
    {
        public RestOptions()
        {
            AdditionalHeaders = new NameValueCollection();
            QueryStringParameters = new NameValueCollection();
        }
        /// <summary>
        /// Additional headers applied to ever REST request
        /// </summary>
        public NameValueCollection AdditionalHeaders { get; set; }
        /// <summary>
        /// Adds these to the request querystring
        /// </summary>
        public NameValueCollection QueryStringParameters { get; set; }

    }
}
namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public enum RestResponseFormat { Xml, Json }
    public enum RestMethod { GET, PUT, POST, DELETE }
    public class RestGetOptions : RestOptions
    {
        public RestGetOptions()
        {
            PathParameters = new NameValueCollection();
        }
        /// <summary>
        /// Substitues path variables in the url with the specified value.  e.g {username} can be specified in this collection with a value and it will replace it.
        /// </summary>
        public NameValueCollection PathParameters { get; set; }
    }
    public class FileUploadOptions
    {

    }
    public class RestPostOptions : RestOptions
    {
        public RestPostOptions()
        {
            PathParameters = new NameValueCollection();
            PostParameters = new NameValueCollection();
        }
        /// <summary>
        /// Substitues path variables in the url with the specified value.  e.g {username} can be specified in this collection with a value and it will replace it.
        /// </summary>
        public NameValueCollection PathParameters { get; set; }
        /// <summary>
        /// These values are added to the  body of the request
        /// </summary>
        public NameValueCollection PostParameters { get; set; }
    }
    public class RestPutOptions : RestOptions
    {
        public RestPutOptions()
        {
            PathParameters = new NameValueCollection();
            PostParameters = new NameValueCollection();
        }
        /// <summary>
        /// Substitues path variables in the url with the specified value.  e.g {username} can be specified in this collection with a value and it will replace it.
        /// </summary>
        public NameValueCollection PathParameters { get; set; }
        /// <summary>
        /// These values are added to the  body of the request
        /// </summary>
        public NameValueCollection PostParameters { get; set; }
    }
    public class RestDeleteOptions : RestOptions
    {
        public RestDeleteOptions()
        {
            PathParameters = new NameValueCollection();

        }
        /// <summary>
        /// Substitues path variables in the url with the specified value.  e.g {username} can be specified in this collection with a value and it will replace it.
        /// </summary>
        public NameValueCollection PathParameters { get; set; }

    }

    public class BatchRequestOptions
    {
        public BatchRequestOptions()
        {
            RunSequentially = false;
        }
        /// <summary>
        /// Additional headers applied to ever REST request
        /// </summary>
        public NameValueCollection AdditionalHeaders { get; set; }
        /// <summary>
        /// Indicates whether the batch requests should be run in order
        /// </summary>
        public bool RunSequentially { get; set; }
    }

    public class RestFileOptions:RestOptions
    {
        /// <summary>
        /// Fired every chunk of file is uploaded providing a percentage of completion
        /// </summary>
        public Action<FileUploadProgress> UploadProgress { get; set; }
    }

    public class BatchRequest
    {
        public BatchRequest(string endpointUrl, int sequence)
        {
            RestMethod = Version1.RestMethod.GET;
            ApiVersion = 2;
            EndpointUrl = endpointUrl;
            Sequence = sequence;
            RequestParameters = new NameValueCollection();
            PathParameters = new NameValueCollection();
        }
        /// <summary>
        /// The API version of REST you wish to access, default is 2
        /// </summary>
        public int ApiVersion { get; set; }
        /// <summary>
        /// The order in which to execute this request, most important when its a sequential request.
        /// </summary>
        public int Sequence { get; set; }
        /// <summary>
        /// Substitues path variables in the url with the specified value.  e.g {username} can be specified in this collection with a value and it will replace it.
        /// </summary>
        public NameValueCollection PathParameters { get; set; }
        private string _endpointUrl = null;

        /// <summary>
        /// The REST endpoint to execute without api.ashx and version
        /// </summary>
        public string EndpointUrl
        {
            get { return _endpointUrl.StartsWith("api.ashx/",StringComparison.CurrentCultureIgnoreCase) ? _endpointUrl :  string.Concat("~/api.ashx/v", this.ApiVersion, "/", _endpointUrl); }
            set
            {
               
                _endpointUrl = value.TrimStart(new[] { '~', '/' });
            }
        }
        /// <summary>
        /// GET,POST,PUT of DELETE
        /// </summary>
        public RestMethod RestMethod { get; set; }
        /// <summary>
        /// Applied to the querystring or body depending on the Http Method
        /// </summary>
        public NameValueCollection RequestParameters { get; set; }

        public override string ToString()
        {
            var data = "";
            if (RequestParameters != null && RequestParameters.HasKeys())
                data = string.Format("&_REQUEST_{0}_DATA={1}", Sequence,
                    HttpUtility.UrlEncode(RequestParameters.MakeQuerystring(false)));

            return string.Concat(string.Format("_REQUEST_{0}_URL={1}&_REQUEST_{0}_METHOD={2}", Sequence, EndpointUrl,
                RestMethod.ToString()), data);
        }

    }
}
