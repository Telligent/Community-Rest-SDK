namespace Telligent.Evolution.RestSDK.Implementations
{
    public class OAuthConfiguration
    {
        public OAuthConfiguration()
        {
            DefaultLanguageKey = "en-us";
            AnonymousUsername = "Anonymous";
            CookieName = "CS-SDK-User";
            LocalUserCreation = new LocalUserConfiguration();
            OauthCallbackUrl = "~/oauth.ashx";
        }
        public string DefaultLanguageKey { get; set; }
        public string AnonymousUsername { get; set; }
        public string OauthClientId { get; set; }
        public string OauthSecret { get; set; }
        public string OauthCallbackUrl { get; set; }
        public string CookieName { get; set; }
        public LocalUserConfiguration LocalUserCreation { get; set; }

    }
}