using aadcapi.Models;
using aadcapi.Utils.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aadcapi.Utils.Authorization
{
    public static class AuthorizeAndFilterByIdentity
    {
        //public static bool IsAuthorizedForThisData(this object Record, string KeyProperty, IEnumerable<string> Roles)
        //{
        //    IFilterFunction filter = RegisteredAuthorizationFilters.GetFilterForType(Record.GetType());
            
        //    return false;
        ////}

        //public static IEnumerable<T> WhereAuthorized<T>(this IEnumerable<T> Collection, IEnumerable<string> Roles)
        //{
        //    IFilterFunction filter = RegisteredAuthorizationFilters.GetFilterForType(typeof(T));
        //    // TODO LOGME need to log when there's no registered filter for the resource type.

        //    return Collection.Where(x => (bool)filter?.IsAuthorized(x, Roles));
        //}
    }

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

    public static class RegisteredRoleControllerRules
    {
        private static List<RoleFilterModel> _models = new List<RoleFilterModel>();
        private static Dictionary<string, List<RoleFilterModel>> _modelsByContext = new Dictionary<string, List<RoleFilterModel>>();
        public static void RegisterRoleControllerModel(this RoleFilterModel Model)
        {
            _models.Add(Model);
            if (!String.IsNullOrEmpty(Model.Context))
            {
                if (!_modelsByContext.ContainsKey(Model.Context))
                {
                    _modelsByContext.Add(Model.Context, new List<RoleFilterModel>());
                }
                _modelsByContext[Model.Context]?.Add(Model);
            }
        }

        public static IEnumerable<RoleFilterModel> GetRoleControllerModels () {
            return _models;
        }

        public static IEnumerable<RoleFilterModel> GetRoleControllerModelsByContext(string Name)
        {
            List<RoleFilterModel> result;
            if (_modelsByContext.TryGetValue(Name, out result))
            {
                return result;
            }

            return null;
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