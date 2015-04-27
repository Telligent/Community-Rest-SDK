using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Telligent.Rest.SDK
{
    public class Encode : IEncode
    {
        public string HtmlAttributeEncode(string htmlAttributeToEncode)
        {
            if (string.IsNullOrEmpty(htmlAttributeToEncode))
                return htmlAttributeToEncode;

            return System.Web.HttpUtility.HtmlAttributeEncode(htmlAttributeToEncode);
        }

        public string HtmlEncode(string htmlToEncode)
        {
            if (string.IsNullOrEmpty(htmlToEncode))
                return htmlToEncode;

            return WebUtility.HtmlEncode(htmlToEncode);
        }

        public string HtmlEnsureEncoded(string htmlToEncode)
        {
            if (string.IsNullOrEmpty(htmlToEncode))
                return htmlToEncode;

            htmlToEncode = _strayAmpRegex.Replace(htmlToEncode, "&amp;");

            htmlToEncode = htmlToEncode.Replace("\"", "&quot;");
            htmlToEncode = htmlToEncode.Replace("\'", "&#39;");
            htmlToEncode = htmlToEncode.Replace("<", "&lt;");
            htmlToEncode = htmlToEncode.Replace(">", "&gt;");

            return htmlToEncode;
        }

        public string UrlEncode(string urlToEncode)
        {
			if (string.IsNullOrEmpty(urlToEncode))
				return urlToEncode;

			// Uri.EscapeDataString only accepts lengths < 32766
			// http://msdn.microsoft.com/en-us/library/system.uri.escapedatastring.aspx
			if(urlToEncode.Length < 32760)
				return Uri.EscapeDataString(urlToEncode).Replace("'", "%27");
	
			var sb = new StringBuilder();
			for (int start = 0; start < urlToEncode.Length; start += 30000)
			{
				int length = Math.Min(30000, urlToEncode.Length - start);
				sb.Append(Uri.EscapeDataString(urlToEncode.Substring(start, length)).Replace("'", "%27"));
			}
			return sb.ToString();
        }

		[Obsolete("This method will be removed in a future version, use UrlEncode(string) instead.", false)]
        public string UrlEncode(string urlToEncode, Encoding encoding)
        {
			return UrlEncode(urlToEncode);
        }

        public string UrlEncodePathComponent(string urlToEncode)
        {
            return UrlEncode(urlToEncode, _pathComponentTextToEscape, '+', '-', '_');
        }

        public string UrlEncodeFileComponent(string urlToEncode)
        {
            return UrlEncode(urlToEncode, _fileComponentTextToEscape, '-', '.', '_');
        }

        public string JavaScriptEncode(string javascript)
        {
            if (String.IsNullOrEmpty(javascript))
                return javascript;

            return System.Web.HttpUtility.JavaScriptStringEncode(javascript);
        }

        private string UrlEncode(string text, Regex pattern, char spaceReplacement, char periodReplacement, char escapePrefix)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            Match match = pattern.Match(text);
            StringBuilder encText = new StringBuilder();
            int lastEndIndex = 0;
            bool escapeAllPeriods = _escapePeriod.IsMatch(text);
            while (match.Value != string.Empty)
            {
                if (lastEndIndex != match.Index)
                    encText.Append(text.Substring(lastEndIndex, match.Index - lastEndIndex));

                if (match.Value == " ")
                    encText.Append(spaceReplacement);
                else if (match.Value == "." && !escapeAllPeriods && match.Index != text.Length - 1 && text.Substring(match.Index + 1, 1) != ".")
                    encText.Append(periodReplacement); // . at the end of text causes a 404... only encode . at the end of text or . preceding other .
                else
                {
                    encText.Append(escapePrefix);
                    byte[] bytes = Encoding.Unicode.GetBytes(match.Value);
                    if (bytes != null)
                    {
                        foreach (byte b in bytes)
                        {
                            string hexByte = b.ToString("X");

                            if (hexByte.Length == 1)
                                encText.Append("0");

                            encText.Append(hexByte);
                        }
                    }
                    encText.Append(escapePrefix);
                }

                lastEndIndex = match.Index + match.Length;
                match = pattern.Match(text, lastEndIndex);
            }

            if (lastEndIndex < text.Length)
                encText.Append(text.Substring(lastEndIndex));

            return encText.ToString();
        }

        #region public static Regex _escapePeriod

        public static Regex _escapePeriod
        {
            get { return __escapePeriod.instance; }
        }

        class __escapePeriod
        {
            static __escapePeriod() { }

            internal static readonly Regex instance = new Regex(@"(?:\.config|\.ascx|\.asax|\.cs|\.vb)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        }

        #endregion

        #region public static Regex _strayAmpRegex

        public static Regex _strayAmpRegex
        {
            get { return __strayAmpRegex.instance; }
        }

        class __strayAmpRegex
        {
            static __strayAmpRegex() { }

            internal static readonly Regex instance = new Regex("&(?!(?:#[0-9]{2,4};|[a-z0-9]+;))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        #endregion

        #region public static Regex _fileComponentTextToEscape

        public static Regex _fileComponentTextToEscape
        {
            get { return __fileComponentTextToEscape.instance; }
        }

        class __fileComponentTextToEscape
        {
            static __fileComponentTextToEscape() { }

            internal static readonly Regex instance = new Regex(@"([^A-Za-z0-9 \.]+|\.| )", RegexOptions.Singleline | RegexOptions.Compiled);
        }

        #endregion

        #region public static Regex _pathComponentTextToEscape

        public static Regex _pathComponentTextToEscape
        {
            get { return __pathComponentTextToEscape.instance; }
        }

        class __pathComponentTextToEscape
        {
            static __pathComponentTextToEscape() { }

            internal static readonly Regex instance = new Regex(@"([^A-Za-z0-9 \.]+|\.| )", RegexOptions.Singleline | RegexOptions.Compiled);
        }

        #endregion
    }
}
