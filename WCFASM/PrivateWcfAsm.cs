using System.IO;
using System.ServiceModel;
using Common;
using WebASM;

namespace WCFASM
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple,
        InstanceContextMode = InstanceContextMode.Single
        )]
    public class PrivateWcfAsm : IServiceContract4J4K
    {
        private readonly PrivateWebAsm m_ActualPrivateAsm;

        public PrivateWcfAsm(PrivateWebAsm actualPrivateAsm)
        {
            m_ActualPrivateAsm = actualPrivateAsm;
        }

        public PrivateWcfAsm(IServe4J actual4J, IServe4K actual4K)
        {
            m_ActualPrivateAsm = new PrivateWebAsm(actual4J, actual4K);
        }

        public Stream GetAuthenticationForToken(string tokenClass, string tokenId)
        {
            var responseEssentials = m_ActualPrivateAsm.VerifyToken(tokenClass, tokenId);
            return WcfWebHelper.PrepareResponse(responseEssentials);
        }

        public Stream InjectSession(Stream bodyStream)
        {
            using (var reader = new StreamReader(bodyStream))
            {
                var sessionXml = reader.ReadToEnd();
                return InjectSession(sessionXml);
            }
        }

        private Stream InjectSession(string sessionXml)
        {
            var responseEssentials = m_ActualPrivateAsm.InjectSession(sessionXml);
            return WcfWebHelper.PrepareResponse(responseEssentials);
        }
    }
}
