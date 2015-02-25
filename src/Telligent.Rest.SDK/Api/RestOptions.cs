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
        public NameValueCollection AdditionalHeaders { get; set; }
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
        public NameValueCollection PathParameters { get; set; }
        public NameValueCollection PostParameters { get; set; }
    }
    public class RestPutOptions : RestOptions
    {
        public RestPutOptions()
        {
            PathParameters = new NameValueCollection();
            PostParameters = new NameValueCollection();
        }
        public NameValueCollection PathParameters { get; set; }
        public NameValueCollection PostParameters { get; set; }
    }
    public class RestDeleteOptions : RestOptions
    {
        public RestDeleteOptions()
        {
            PathParameters = new NameValueCollection();

        }
        public NameValueCollection PathParameters { get; set; }

    }

    public class BatchRequestOptions
    {
        public BatchRequestOptions()
        {
            RunSequentially = false;
        }
        public NameValueCollection AdditionalHeaders { get; set; }
        public bool RunSequentially { get; set; }
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
        public int ApiVersion { get; set; }
        public int Sequence { get; set; }
        public NameValueCollection PathParameters { get; set; }
        private string _endpointUrl = null;

        public string EndpointUrl
        {
            get { return string.Concat("~/api.ashx/v", this.ApiVersion, "/", _endpointUrl); }
            set
            {
               
                _endpointUrl = value.TrimStart(new[] { '~', '/' });
            }
        }
        public RestMethod RestMethod { get; set; }
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
