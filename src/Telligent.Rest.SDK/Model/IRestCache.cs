using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Telligent.Rest.SDK.Model
{
    public interface IRestCache
    {
        void Put(string key, object value, int cacheDurationSeconds);
        void Remove(string key);
        object Get(string key);
        void Clear();
    }
}
