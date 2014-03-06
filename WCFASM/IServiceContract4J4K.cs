using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WCFASM
{
    [ServiceContract]
    public interface IServiceContract4J4K
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Class/{tokenClass}/Token/{tokenId}/Authentication")]
        Stream GetAuthenticationForToken(string tokenClass, string tokenId);

        [OperationContract]
        [WebInvoke(UriTemplate = "/Session", Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream InjectSession(Stream sessionXml);
    }
}