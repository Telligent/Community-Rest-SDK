using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;


namespace Telligent.Evolution.Extensions.OAuthAuthentication.Services
{
	internal interface IUserSyncService
	{
        bool GetCreateUser(IUserCreatableOAuthClientConfiguration configuration);
	}
}
