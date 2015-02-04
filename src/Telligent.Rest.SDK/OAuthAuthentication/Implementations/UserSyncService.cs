using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Telligent.Evolution.Extensibility.OAuthClient.Version1;

using Telligent.Evolution.Extensions.OAuthAuthentication.Services;

namespace Telligent.Evolution.Extensions.OAuthAuthentication.Implementations
{
    
    internal class UserSyncService : IUserSyncService
    {
     
        #region IUserSyncService Members

        public bool GetCreateUser(IUserCreatableOAuthClientConfiguration configuration)
        {
            var userExists = UserExists(configuration);
            if (userExists)
                return true;

            //Create the user
            var userCreated = CreateUser(configuration);
            if (userCreated)
                return true;

            return false;

        }
        #endregion
        private bool CreateUser(IUserCreatableOAuthClientConfiguration configuration)
        {
            User user = null;
            try
            {
                user = GetToken(configuration, configuration.EvolutionUserCreationManagementUserName);
            }
            catch (Exception)
            {
                configuration.UserCreationFailed(configuration.LocalUserName, configuration.LocalUserEmailAddress, configuration.LocalUserDetails, "There was an error retrieving the management user",ErrorResponse.UnknownError);
                return false;
            }

            if (user != null)
            {
                try
                {

                    //First lets see if the email is in use
                    bool emailExists = IsEmailInUse(configuration, user);

                    if(emailExists)
                    {
                        configuration.UserCreationFailed(configuration.LocalUserName, configuration.LocalUserEmailAddress, configuration.LocalUserDetails, "The email address specified is already in use", ErrorResponse.DuplicateEmail);
                        return false;
                    }

                    var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/users.xml");
                    request.Timeout = Constants.RequestTimeoutMilliseconds;
					if (configuration.EvolutionCredentials != null)
					{
						request.Credentials = configuration.EvolutionCredentials;
						request.Headers["OAuth-Authorization"] = user.OAuthToken;
					}
					else
						request.Headers["Authorization"] = "OAuth " + user.OAuthToken;

                    request.Method = "POST";
                    

                    int defaultPasswordLength = 6;
                    int defaultNonAlphaChars = 0;

                    try
                    {
                        if (System.Web.Security.Membership.MinRequiredPasswordLength > 0)
                            defaultPasswordLength = System.Web.Security.Membership.MinRequiredPasswordLength;

                        defaultNonAlphaChars = System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters;
                    }
                    catch
                    {
                       defaultPasswordLength = 6;
                       defaultNonAlphaChars = 0;
                    }


                    string password = System.Web.Security.Membership.GeneratePassword(defaultPasswordLength, defaultNonAlphaChars);
                    string data = string.Concat(
                        "Username=",
                        Uri.EscapeDataString(configuration.LocalUserName),
                        "&Password=",
                        Uri.EscapeDataString(password),
                        "&PrivateEmail=",
                        Uri.EscapeDataString(configuration.LocalUserEmailAddress)
                        );

                    if (configuration.LocalUserDetails != null && configuration.LocalUserDetails.Count > 0)
                    {
                        var fields = new List<string>();
                        foreach (string key in configuration.LocalUserDetails.Keys)
                        {
                            string profileFieldKey = "_ProfileFields_" + Uri.EscapeDataString(key);
                            string fieldValue = Uri.EscapeUriString(configuration.LocalUserDetails[key]);
                            fields.Add(profileFieldKey + "=" + fieldValue);
                        }

                        string profileDataString = string.Join("&", fields);
                        data = data + "&" + profileDataString;
                    }

                    byte[] bytes = Encoding.UTF8.GetBytes(data);

                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = bytes.Length;

                    using (var requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(bytes, 0, bytes.Length);
                        requestStream.Close();
                    }

                    string rawResponse = null;

                    using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                    {
                        using (var reader = new StreamReader(webResponse.GetResponseStream()))
                        {
                            rawResponse = reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException ex)
                {
                    string errResponse = null;
                    using (var reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        errResponse = reader.ReadToEnd();
                    }

                    ErrorResponse err = ErrorResponse.UnknownError;
                    var errMsg = ParseErrorMessage(errResponse, out err);
                    configuration.UserCreationFailed(configuration.LocalUserName, configuration.LocalUserEmailAddress, configuration.LocalUserDetails, errMsg,err);
                    return false;

                }
                catch
                {
                    configuration.UserCreationFailed(configuration.LocalUserName, configuration.LocalUserEmailAddress, configuration.LocalUserDetails, "There was an error creating the user", ErrorResponse.UnknownError);
                    return false;
                }
            }


            return true;
        }
        private string ParseErrorMessage(string resp, out ErrorResponse error)
        {
            error = ErrorResponse.UnknownError;

            if (string.IsNullOrEmpty(resp))
                return null;

            var response = XElement.Parse(resp).Element("Errors");
            if (response == null)
                return null;

            var msg = response.Descendants("Message").FirstOrDefault();
            if (msg != null)
            {
                var errorMsg = msg.Value.ToLower();
                if(errorMsg.Contains("(username)"))
                {
                    error = ErrorResponse.InvalidUsername;
                }
                else if(errorMsg.Contains("(email)"))
                {
                    error = ErrorResponse.InvalidEmail;
                }

                return msg.Value;
            }

            return null;
        }
        private User GetToken(IUserCreatableOAuthClientConfiguration configuration, string username)
        {
            var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/oauth/token");
            request.Timeout = Constants.RequestTimeoutMilliseconds;

            if (configuration.EvolutionCredentials != null)
                request.Credentials = configuration.EvolutionCredentials;

            request.Method = "POST";

            string data = string.Concat(
                "client_id=",
                Uri.EscapeDataString(configuration.OAuthClientId),
                "&client_secret=",
                Uri.EscapeDataString(configuration.OAuthClientSecret),
                "&grant_type=client_credentials&username=",
                Uri.EscapeDataString(username)
                );

            byte[] bytes = Encoding.UTF8.GetBytes(data);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = bytes.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
            }

            string rawResponse = null;

            using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    rawResponse = reader.ReadToEnd();
                }
            }


            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var response = serializer.Deserialize<OAuthResponse>(rawResponse);

            if (!string.IsNullOrEmpty(response.error))
                return null;

            var user = new User(configuration.DefaultUserName,-1, configuration.DefaultUserLanguageKey);
            user.OAuthToken = response.access_token;
            user.RefreshToken = response.refresh_token;
            user.TokenExpiresUtc = DateTime.UtcNow.AddSeconds(response.expires_in);

            return user;
        }
        private bool UserExists(IUserCreatableOAuthClientConfiguration configuration)
        {
            try
            {
                var user = GetToken(configuration, configuration.LocalUserName);
                return user != null;
            }
            catch
            {
                return false;
            }

        }

        private bool IsEmailInUse(IUserCreatableOAuthClientConfiguration configuration, User managementAccount)
       {
           var request = (HttpWebRequest)WebRequest.Create(configuration.EvolutionBaseUrl.OriginalString + "api.ashx/v2/users.xml?PageIndex=0&PageSize=1&EmailAddress=" + HttpUtility.UrlEncode(configuration.LocalUserEmailAddress));
           request.Timeout = Constants.RequestTimeoutMilliseconds;

		   if (configuration.EvolutionCredentials != null)
		   {
			   request.Credentials = configuration.EvolutionCredentials;
			   request.Headers["OAuth-Authorization"] = managementAccount.OAuthToken;
		   }
		   else
			   request.Headers["Authorization"] = "OAuth " + managementAccount.OAuthToken;

           request.Method = "GET";

           string rawResponse = null;
           try
           {
               using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
               {
                   using (var reader = new StreamReader(webResponse.GetResponseStream()))
                   {
                       rawResponse = reader.ReadToEnd();
                   }
               }
           }
           catch
           {
               return false; // We didn't get a user but something went wrong
           }

           try
           {
               var xml = XElement.Parse(rawResponse);
               var user = xml.Descendants("User").FirstOrDefault(); // If we have a user node then its a duplicate
               return user != null;
           }
           catch
           {
               return false; // We got a response but not in the expected format
           }

           
       }


    }
}
