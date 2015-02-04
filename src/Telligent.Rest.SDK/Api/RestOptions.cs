using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telligent.Rest.SDK;

namespace Telligent.Rest.SDK
{
    public abstract class RestOptions
    {
        NameValueCollection AdditionalHeaders { get; set; }
    }
}
namespace Telligent.Evolution.Extensibility.Rest.Version1
{
    public enum RestResponseFormat { Xml,Json}

    public class RestGetOptions :RestOptions
    {
    }
    public class RestPostOptions : RestOptions
    {
    }
    public class RestPutOptions : RestOptions
    {
    }
    public class RestDeleteOptions : RestOptions
    {
    }
}
