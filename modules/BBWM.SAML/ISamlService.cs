namespace BBWM.SAML
{
    public interface ISamlService
    {
        string RedirectToProvider();

        SamlUser ParseResponse(string response);
    }
}
