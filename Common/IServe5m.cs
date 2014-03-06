using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public interface IServe5M
    {
        SeacToken GetToken(string tokenClass, string inToken, string inClass);
        string Auth5LTokenClass { get; }
        string Auth5LRealm { get; }
    }
}
