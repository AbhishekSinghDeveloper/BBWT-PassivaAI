using System.Security.Cryptography.X509Certificates;

namespace BBWM.SAML
{
    public class Certificate
    {
        public X509Certificate2 cert;

        public void LoadCertificate(string certificate)
        {
            LoadCertificate(StringToByteArray(certificate));
        }

        public void LoadCertificate(byte[] certificate)
        {
            cert = new X509Certificate2(new X509Certificate(certificate));
        }

        private byte[] StringToByteArray(string st)
        {
            var bytes = new byte[st.Length];
            for (var i = 0; i < st.Length; i++)
            {
                bytes[i] = (byte)st[i];
            }
            
            return bytes;
        }
    }
}