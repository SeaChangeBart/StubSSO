using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WCFASM
{
    [ServiceContract]
    public interface IServiceContract4J
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Class/{tokenClass}/Token/{tokenId}/Authentication")]
        String GetAuthenticationForToken(string tokenClass, string tokenId);
    }
}
