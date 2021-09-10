using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aadcapi.Utils.Authorization
{
    public interface IFilterFunction
    {
        bool IsAuthorized(object Resource, IEnumerable<string> Roles);
    }
}
