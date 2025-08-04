using System;
using System.Text;
using System.Xml;
using System.Security.Cryptography.Xml;

namespace BBWM.SAML
{
    public class Response
    {
        private XmlDocument _xmlDoc;
        private Certificate _certificate;
        private XmlNamespaceManager _xmlNameSpaceManager; //we need this one to run our XPath queries on the SAML XML

        public string Xml { get { return _xmlDoc.OuterXml; } }

        public Response(string certificateStr)
        {
            _certificate = new Certificate();
            _certificate.LoadCertificate(certificateStr);
        }

        public void LoadXml(string xml)
        {
            _xmlDoc = new XmlDocument();
            _xmlDoc.PreserveWhitespace = true;
            _xmlDoc.XmlResolver = null;
            _xmlDoc.LoadXml(xml);

            _xmlNameSpaceManager = GetNamespaceManager(); //lets construct a "manager" for XPath queries
        }

        public void LoadXmlFromBase64(string response)
        {
            LoadXml(Encoding.UTF8.GetString(Convert.FromBase64String(response)));
        }

        public bool IsValid()
        {
            var nodeList = _xmlDoc.SelectNodes("//ds:Signature", _xmlNameSpaceManager);
            var signedXml = new SignedXml(_xmlDoc);

            if (nodeList.Count > 0)
            {
                signedXml.LoadXml((XmlElement)nodeList[0]);
                return ValidateSignatureReference(signedXml) && signedXml.CheckSignature(_certificate.cert, true) && !IsExpired();
            }

            return false;
        }

        //an XML signature can "cover" not the whole document, but only a part of it
        //.NET's built in "CheckSignature" does not cover this case, it will validate to true.
        //We should check the signature reference, so it "references" the id of the root document element! If not - it's a hack
        private bool ValidateSignatureReference(SignedXml signedXml)
        {
            if (signedXml.SignedInfo.References.Count == 1) //no ref at all
            {
                var reference = (Reference)signedXml.SignedInfo.References[0];
                var id = reference.Uri.Substring(1);

                var idElement = signedXml.GetIdElement(_xmlDoc, id);

                if (idElement != _xmlDoc.DocumentElement)
                {
                    var assertionNode = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion", _xmlNameSpaceManager) as XmlElement;
                    return assertionNode == idElement;
                }

                return true;
            }

            return false;
        }

        private bool IsExpired()
        {
            var expirationDate = DateTime.MaxValue;
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:SubjectConfirmation/saml:SubjectConfirmationData", _xmlNameSpaceManager);
            if (node != null && node.Attributes["NotOnOrAfter"] != null)
            {
                DateTime.TryParse(node.Attributes["NotOnOrAfter"].Value, out expirationDate);
            }

            return DateTime.UtcNow > expirationDate.ToUniversalTime();
        }

        public string GetNameID()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:Subject/saml:NameID", _xmlNameSpaceManager);
            return node.InnerText;
        }

        public string GetEmail()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='User.email']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='Email']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']/saml:AttributeValue", _xmlNameSpaceManager);

            return node?.InnerText;
        }

        public string GetFirstName()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='first_name']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='FirstName']/saml:AttributeValue", _xmlNameSpaceManager);

            return node?.InnerText;
        }

        public string GetLastName()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='last_name']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='LastName']/saml:AttributeValue", _xmlNameSpaceManager);

            return node?.InnerText;
        }

        public string GetDepartment()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/department']/saml:AttributeValue", _xmlNameSpaceManager);
            return node?.InnerText;
        }

        public string GetPhone()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/homephone']/saml:AttributeValue", _xmlNameSpaceManager) ??
                _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/telephonenumber']/saml:AttributeValue", _xmlNameSpaceManager);
            
            return node?.InnerText;
        }

        public string GetCompany()
        {
            var node = _xmlDoc.SelectSingleNode("/samlp:Response/saml:Assertion/saml:AttributeStatement/saml:Attribute[@Name='http://schemas.xmlsoap.org/ws/2005/05/identity/claims/companyname']/saml:AttributeValue", _xmlNameSpaceManager);
            return node?.InnerText;
        }

        //returns namespace manager, we need one b/c MS says so... Otherwise XPath doesnt work in an XML doc with namespaces
        //see https://stackoverflow.com/questions/7178111/why-is-xmlnamespacemanager-necessary
        private XmlNamespaceManager GetNamespaceManager()
        {
            var manager = new XmlNamespaceManager(_xmlDoc.NameTable);
            manager.AddNamespace("ds", SignedXml.XmlDsigNamespaceUrl);
            manager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            manager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");

            return manager;
        }
    }
}