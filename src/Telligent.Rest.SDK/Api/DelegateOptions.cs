using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;

namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    /// <summary>
    /// For future use
    /// </summary>
    public class ResolveLocalUserArgs
    {
        public ResolveLocalUserArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public ResolveLocalUserArgs()
        {
                
        }
        public HttpContextBase HttpContext { get; set; }
    }
    /// <summary>
    /// For future use
    /// </summary>
    public class RedirectUrlArgs
    {
        public RedirectUrlArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public RedirectUrlArgs()
        {
                
        }
        public HttpContextBase HttpContext { get; set; }
    }
    /// <summary>
    /// For future use
    /// </summary>
    public class UserCreationFailedArgs
    {
        public UserCreationFailedArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public UserCreationFailedArgs()
        {

        }
        public HttpContextBase HttpContext { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public IDictionary<string, string> AdditionalData { get; set; }
        public string Message { get; set; }
    }
    /// <summary>
    /// For future use
    /// </summary>
    public class OAuthLoginFailedArgs
    {
        public OAuthLoginFailedArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public OAuthLoginFailedArgs()
        {

        }
        public HttpContextBase HttpContext { get; set; }
        public NameValueCollection State { get; set; }
    }
    /// <summary>
    /// For future use
    /// </summary>
    public class OAuthLogoutFailedArgs
    {
        public OAuthLogoutFailedArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public OAuthLogoutFailedArgs()
        {

        }
       public HttpContextBase HttpContext { get; set; }
       public NameValueCollection State { get; set; }
    }

    public class OAuthUserLoggedInArgs
    {
        public OAuthUserLoggedInArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public OAuthUserLoggedInArgs()
        {

        }
        public HttpContextBase HttpContext { get; set; }
        public NameValueCollection State { get; set; }
        public User User { get; set; }
    }
    public class OAuthUserLoggedOutArgs
    {
        public OAuthUserLoggedOutArgs(HttpContextBase httpContext)
        {
            HttpContext = httpContext;
        }

        public OAuthUserLoggedOutArgs()
        {

        }
        public HttpContextBase HttpContext { get; set; }
        public NameValueCollection State { get; set; }

    }
}
