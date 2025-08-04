namespace BBWM.SAML
{
    public class SamlSettings
    {
        public bool Enabled { get; set; }

        public string Issuer { get; set; }

        public string ConsumerUrl { get; set; }

        public string SamlEndpoint { get; set; }

        public string Certificate { get; set; }
    }
}
