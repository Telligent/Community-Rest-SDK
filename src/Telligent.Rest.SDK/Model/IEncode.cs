using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telligent.Rest.SDK
{
    public interface IEncode
    {
        string HtmlAttributeEncode(string htmlAttributeToEncode);

        string HtmlEncode(string htmlToEncode);
        string HtmlEnsureEncoded(string htmlToEncode);

        string UrlEncode(string urlToEncode);
		[Obsolete("This method will be removed in a future version, use UrlEncode(string) instead.", false)]
        string UrlEncode(string urlToEncode, Encoding encoding);
        string UrlEncodePathComponent(string urlToEncode);
        string UrlEncodeFileComponent(string urlToEncode);

        string JavaScriptEncode(string javascript);
    }
}
