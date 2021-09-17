using aadcapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace aadcapi.Utils.Authorization
{
    public static class RegisteredRoleControllerRules
    {
        private static readonly object _lock = new object();

        private static Dictionary<Guid,RoleFilterModel> _models = new Dictionary<Guid, RoleFilterModel>();
        private static Dictionary<string, List<Guid>> _modelsByContext = new Dictionary<string, List<Guid>>();
        
        public static void RegisterRoleControllerModel(this RoleFilterModel Model)
        {
            lock (_lock)
            {
                if (_models.ContainsKey(Model.Id))
                {
                    var old = _models[Model.Id];

                    // Remove what will be a stale entry in the modelsByContext Dictionary
                    if (!old.Context.Equals(Model.Context, StringComparison.CurrentCultureIgnoreCase)
                        && _modelsByContext.ContainsKey(old.Context))
                    {
                        _modelsByContext[old.Context]?.Remove(Model.Id);
                    }
                    old = null;

                    // Update _model with new record
                    _models[Model.Id] = Model;
                }
                else
                {
                    // Add new _model
                    _models.Add(Model.Id, Model);
                }

                if (!String.IsNullOrEmpty(Model.Context))
                {
                    if (!_modelsByContext.ContainsKey(Model.Context))
                    {
                        _modelsByContext.Add(Model.Context, new List<Guid>() { Model.Id });
                        return;
                    }

                    _modelsByContext[Model.Context]?.Add(Model.Id);
                    var models = _modelsByContext[Model.Context].Distinct().ToList();
                    _modelsByContext[Model.Context] = models;
                }
            }
        }

        public static IEnumerable<RoleFilterModel> GetRoleControllerModels () {
            lock (_lock)
            {
                return _models.Values.ToArray();
            }
        }

        public static IEnumerable<RoleFilterModel> GetRoleControllerModelsByContext(string Name)
        {
            lock (_lock)
            {
                List<Guid> ids;
                if (_modelsByContext.TryGetValue(Name, out ids))
                {
                    return ids.Select(x => {
                        RoleFilterModel model;
                        if (_models.TryGetValue(x, out model))
                        {
                            return model;
                        }
                        return null;
                    }).Where(x => x != null).ToArray();
                }
            }

            return null;
        }
    }
}