using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WCFASM
{
    [ServiceContract]
    public interface IServiceContract4K
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/Session", Method="POST", BodyStyle = WebMessageBodyStyle.Bare)]
        String InjectSession(String sessionXml);
    }
}