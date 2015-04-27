using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web;

namespace Telligent.Rest.SDK
{
    public class Decode : IDecode
    {
        public string HtmlDecode(string htmlToDecode)
        {
            if (string.IsNullOrEmpty(htmlToDecode))
                return htmlToDecode;

            return WebUtility.HtmlDecode(htmlToDecode);
        }

        public string UrlDecode(string urlToDecode)
        {
			if (string.IsNullOrEmpty(urlToDecode))
				return urlToDecode;

			// Uri.UnescapeDataString only accepts lengths < 32766, using the same code here to be safe
			// http://msdn.microsoft.com/en-us/library/system.uri.escapedatastring.aspx
			if (urlToDecode.Length < 32760)
				return Uri.UnescapeDataString(urlToDecode.Replace("+", " "));

        	int start = 0;
			var sb = new StringBuilder();
			while(start < urlToDecode.Length)
			{
				int length = Math.Min(30000, urlToDecode.Length - start);
				if (urlToDecode[start + length - 1] == '%')
					length--;
				else if (urlToDecode[start + length - 2] == '%')
					length -= 2;

				string unescapedString = null;
				bool threwException = true;
				while (threwException)
				{
					try
					{
						unescapedString = Uri.UnescapeDataString(urlToDecode.Substring(start, length).Replace("+", " "));
						threwException = false;
					}
					catch (UriFormatException)
					{
						threwException = true;
						if (length >= 3)
							length -= 3;
						else
							break;
					}
				}

				if(!string.IsNullOrEmpty(unescapedString))
					sb.Append(unescapedString);
				start += length;
			}
			return sb.ToString();
        }

		[Obsolete("This method will be removed in a future version, use UrlDecode(string) instead.", false)]
        public string UrlDecode(string urlToDecode, Encoding encoding)
        {
			return UrlDecode(urlToDecode);
        }

        public string UrlDecodePathComponent(string urlToDecode)
        {
            return UrlDecode(urlToDecode, _pathComponentTextToUnescape, '+', '-');
        }

        public string UrlDecodeFileComponent(string urlToDecode)
        {
            return UrlDecode(urlToDecode, _fileComponentTextToUnescape, '-', '.');
        }

        private static string UrlDecode(string text, Regex pattern, char spaceReplacement, char periodReplacement)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            Match match = pattern.Match(text);
            StringBuilder decText = new StringBuilder();
            int lastEndIndex = 0;
            while (match.Value != string.Empty)
            {
                if (lastEndIndex != match.Index)
                    decText.Append(text.Substring(lastEndIndex, match.Index - lastEndIndex));

                if (match.Value.Length == 1)
                {
                    if (match.Value[0] == spaceReplacement)
                        decText.Append(" ");
                    else if (match.Value[0] == periodReplacement)
                        decText.Append(".");
                }
                else
                {
                    byte[] bytes = new byte[(match.Value.Length - 2) / 2];

                    for (int i = 1; i < match.Value.Length - 1; i += 2)
                        bytes[(i - 1) / 2] = byte.Parse(match.Value.Substring(i, 2), NumberStyles.AllowHexSpecifier);

                    decText.Append(Encoding.Unicode.GetString(bytes));
                }

                lastEndIndex = match.Index + match.Length;
                match = pattern.Match(text, lastEndIndex);
            }

            if (lastEndIndex < text.Length)
                decText.Append(text.Substring(lastEndIndex));

            return decText.ToString();
        }

        #region public static Regex _pathComponentTextToUnescape

        public static Regex _pathComponentTextToUnescape
        {
            get { return __pathComponentTextToUnescape.instance; }
        }

        class __pathComponentTextToUnescape
        {
            static __pathComponentTextToUnescape() { }

            internal static readonly Regex instance = new Regex(@"((?:_(?:[0-9a-f][0-9a-f][0-9a-f][0-9a-f])+_)|\-|\+)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion

        #region public static Regex _fileComponentTextToUnescape

        public static Regex _fileComponentTextToUnescape
        {
            get { return __fileComponentTextToUnescape.instance; }
        }

        class __fileComponentTextToUnescape
        {
            static __fileComponentTextToUnescape() { }

            internal static readonly Regex instance = new Regex(@"((?:_(?:[0-9a-f][0-9a-f][0-9a-f][0-9a-f])+_)|_|\-)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion
    }
}
