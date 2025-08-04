using System;
using System.Web;
using System.IO;
using System.Xml;
using System.IO.Compression;
using System.Text;

namespace BBWM.SAML
{
    public class AuthRequest
    {
        public string _id;
        private readonly string _issue_instant;

        private readonly string _issuer;
        private readonly string _assertionConsumerServiceUrl;

        public enum AuthRequestFormat
        {
            Base64 = 1
        }

        public AuthRequest(string issuer, string assertionConsumerServiceUrl)
        {
            _id = $"_{Guid.NewGuid().ToString()}";
            _issue_instant = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            _issuer = issuer;
            _assertionConsumerServiceUrl = assertionConsumerServiceUrl;
        }

        public string GetRequest(AuthRequestFormat format)
        {
            using (var sw = new StringWriter())
            {
                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;

                using (var xw = XmlWriter.Create(sw, xws))
                {
                    xw.WriteStartElement("samlp", "AuthnRequest", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("ID", _id);
                    xw.WriteAttributeString("Version", "2.0");
                    xw.WriteAttributeString("IssueInstant", _issue_instant);
                    xw.WriteAttributeString("ProtocolBinding", "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST");
                    xw.WriteAttributeString("AssertionConsumerServiceURL", _assertionConsumerServiceUrl);

                    xw.WriteStartElement("saml", "Issuer", "urn:oasis:names:tc:SAML:2.0:assertion");
                    xw.WriteString(_issuer);
                    xw.WriteEndElement();

                    xw.WriteStartElement("samlp", "NameIDPolicy", "urn:oasis:names:tc:SAML:2.0:protocol");
                    xw.WriteAttributeString("Format", "urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified");
                    xw.WriteAttributeString("AllowCreate", "true");
                    xw.WriteEndElement();

                    xw.WriteEndElement();
                }

                if (format == AuthRequestFormat.Base64)
                {
                    //https://stackoverflow.com/questions/25120025/acs75005-the-request-is-not-a-valid-saml2-protocol-message-is-showing-always%3C/a%3E
                    var memoryStream = new MemoryStream();
                    var writer = new StreamWriter(new DeflateStream(memoryStream, CompressionMode.Compress, true), new UTF8Encoding(false));
                    writer.Write(sw.ToString());
                    writer.Close();
                    return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length, Base64FormattingOptions.None);
                }

                return null;
            }
        }

        //returns the URL you should redirect your users to (i.e. your SAML-provider login URL with the Base64-ed request in the querystring
        public string GetRedirectUrl(string samlEndpoint)
        {
            var separator = samlEndpoint.Contains("?") ? "&" : "?";
            return $"{samlEndpoint}{separator}SAMLRequest={HttpUtility.UrlEncode(GetRequest(AuthRequestFormat.Base64))}";
        }
    }
}