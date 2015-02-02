namespace Telligent.Evolution.RestSDK.Implementations
{
    public class SSOConfiguration
    {
        public SSOConfiguration()
        {
            Enabled = false;
            SynchronizationCookieName = "EvolutionSync";
        }    
        public bool Enabled { get; set; }
        public string SynchronizationCookieName { get; set; }
    }
}