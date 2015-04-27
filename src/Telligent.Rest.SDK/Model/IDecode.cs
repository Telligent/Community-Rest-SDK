using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Telligent.Rest.SDK
{
    public interface IDecode
    {
        string HtmlDecode(string htmlToDecode);

        string UrlDecode(string urlToDecode);
		[Obsolete("This method will be removed in a future version, use UrlEncode(string) instead.", false)]
        string UrlDecode(string urlToDecode, Encoding encoding);
        string UrlDecodePathComponent(string urlToDecode);
        string UrlDecodeFileComponent(string urlToDecode);
    }
}
