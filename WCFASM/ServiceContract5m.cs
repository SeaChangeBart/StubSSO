using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace WCFASM
{
    [ServiceContract]
    public interface IServiceContract5M
    {
        [OperationContract]
        [WebGet(UriTemplate = "/Class/{tokenClass}/Token")]
        Stream GetToken(string tokenClass);
    }
}