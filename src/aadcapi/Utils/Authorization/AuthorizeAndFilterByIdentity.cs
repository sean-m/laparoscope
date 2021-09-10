using aadcapi.Utils.Authorization;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aadcapi.Utils.Authorization
{
    public static class RegisteredAuthorizationFilters
    {   
        private static readonly Dictionary<Type, IFilterFunction> _filters = new Dictionary<Type, IFilterFunction>();

        public static void RegisterFilter(Type ResourceType, IFilterFunction FilterFunction)
        {
            _filters.Add(ResourceType, FilterFunction);
        }

        public static IFilterFunction GetFilterForType(Type ResourceType)
        {
            if (_filters.ContainsKey(ResourceType))
            {
                return _filters[ResourceType];
            }

            return default(IFilterFunction);
        }
    }
    public class FilterFunction<T> : IFilterFunction
    {
        public T ForType { get; private set; }
        public IEnumerable<string> ForRoles {
            get; private set;
        } = new List<string>();

        public Func<T, bool> Filter { get; private set; }

        public FilterFunction(Func<T, bool> Filter)
                
        {
            this.Filter = Filter;
        }

        public void AddRole(string Role) => ((List<string>)this.ForRoles).Add(Role);

        public bool IsAuthorized(object Resource, IEnumerable<string> Roles)
        {
            if (Resource is T resource)
            {
                return Roles.Any(x => ForRoles.Any(y => y.Equals(x, StringComparison.CurrentCultureIgnoreCase))) && Filter(resource);
            }

            return false;
        }

    }
}