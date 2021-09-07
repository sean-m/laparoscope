using aadcapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aadcapi.Utils
{
    public static class AuthorizeAndFilterByIdentity
    {
        //public static bool IsAuthorizedForThisData(this object Record, string KeyProperty, IEnumerable<string> Roles)
        //{
        //    IFilterFunction filter = RegisteredAuthorizationFilters.GetFilterForType(Record.GetType());
            
        //    return false;
        //}

        public static IEnumerable<T> WhereAuthorized<T>(this IEnumerable<T> Collection, IEnumerable<string> Roles)
        {
            IFilterFunction filter = RegisteredAuthorizationFilters.GetFilterForType(typeof(T));
            // TODO LOG need to log when there's no registered filter for the resource type.

            return Collection.Where(x => (bool)filter?.IsAuthorized(x, Roles));
        }
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
        private static List<RoleControllerModel> _models = new List<RoleControllerModel>();
        private static Dictionary<string, List<RoleControllerModel>> _modelsByController = new Dictionary<string, List<RoleControllerModel>>();
        public static void RegisterRoleControllerModel(this RoleControllerModel Model)
        {
            _models.Add(Model);
            if (!String.IsNullOrEmpty(Model.Context))
            {
                if (!_modelsByController.ContainsKey(Model.Context))
                {
                    _modelsByController.Add(Model.Context, new List<RoleControllerModel>());
                }
                _modelsByController[Model.Context]?.Add(Model);
            }
        }

        public static IEnumerable<RoleControllerModel> GetRoleControllerModels () {
            return _models;
        }

        public static IEnumerable<RoleControllerModel> GetRoleControllerModelsByController(string Name)
        {
            List<RoleControllerModel> result;
            if (_modelsByController.TryGetValue(Name, out result))
            {
                return result;
            }

            return null;
        }
    }

    public interface IFilterFunction
    {
        bool IsAuthorized(object Resource, IEnumerable<string> Roles);
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