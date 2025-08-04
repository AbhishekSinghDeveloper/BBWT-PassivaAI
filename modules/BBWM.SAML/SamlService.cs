using Microsoft.Extensions.Options;

namespace BBWM.SAML
{
    public class SamlService: ISamlService
    {
        private readonly IOptionsSnapshot<SamlSettings> _samlSettings;

        public SamlService(IOptionsSnapshot<SamlSettings> samlSettings)
        {
            _samlSettings = samlSettings;
        }

        public string RedirectToProvider()
        {
            var config = _samlSettings.Value;
            if (config != null && config.Enabled)
            {
                var request = new AuthRequest(config.Issuer, config.ConsumerUrl);
                return request.GetRedirectUrl(config.SamlEndpoint);
            }

            return null;
        }

        public SamlUser ParseResponse(string responseString)
        {
            var config = _samlSettings.Value;
            if (config != null && config.Enabled)
            {
                var response = new Response(config.Certificate);
                response.LoadXmlFromBase64(responseString);

                if (response.IsValid())
                {
                    return new SamlUser
                    {
                        Username = response.GetNameID(),
                        FirstName = response.GetFirstName(),
                        LastName = response.GetLastName(),
                        Phone = response.GetPhone(),
                        Email = response.GetEmail()
                    };
                }
            }

            return null;
        }
    }
}
