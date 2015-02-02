using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.Rest.Version1;
using System.Xml;


namespace Telligent.Rest.SDK.Model
{
    public interface IRestHostRegistrationService
    {
		void Register(RestHost host);
		RestHost Get(Guid id);
		void Remove(Guid id);
    }
}
