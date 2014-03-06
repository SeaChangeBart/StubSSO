using System.Collections.Generic;
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
    public class PublicWcfAsm : IServiceContract5M
    {
        private readonly PublicWebAsm m_ActualPublicAsm;
        public PublicWcfAsm(PublicWebAsm actualPublicAsm)
        {
            m_ActualPublicAsm = actualPublicAsm;
        }

        public PublicWcfAsm(IServe5M actual5M)
        {
            m_ActualPublicAsm = new PublicWebAsm(actual5M);
        }

        public Stream GetToken(string tokenClass)
        {
            var requestEssentials = WcfWebHelper.GetRequestEssentials();
            var responseEssentials = m_ActualPublicAsm.GetToken(tokenClass, requestEssentials);
            return WcfWebHelper.PrepareResponse(responseEssentials);
        }
    }
}
