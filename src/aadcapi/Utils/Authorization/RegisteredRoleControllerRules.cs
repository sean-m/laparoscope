using aadcapi.Models;
using System;
using System.Collections.Generic;

namespace aadcapi.Utils.Authorization
{
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
}