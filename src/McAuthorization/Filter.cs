using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace McAuthorization
{
    public static class Filter
    {
        /// <summary>
        /// For a given Context, an list of Roles, retrieve any matching rules and evaluate them
        /// against the Model. For AADC controllers it would be obvious
        /// to check if there is a rule that specifies a role that should allow access to
        /// connectors with a given name. The rule would for admins accessing my lab connector
        /// would look like this:
        ///   Role: "Admin"
        ///   ModelProperty: "Name"
        ///   ModelValue: "garage.mcardletech.com"  // actual example used in my lab not shilling my website,
        ///                                         // it's truly not worth your time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Model"></param>
        /// <param name="Context"></param>
        /// <param name="Roles"></param>
        /// <returns></returns>
        public static bool IsAuthorized<T>(T Model, string Context, IEnumerable<string> Roles)
        {
            var rules = RegisteredRoleControllerRules.GetRoleControllerModelsByContext(Context);
            var connType = typeof(T);
            if (connType == typeof(object)) connType = Model.GetType();
            
            return (bool) rules.Where(
                    rule => {
                        bool result = (bool)Roles?.Any(
                            role => {
                                var match = role.Like(rule.Role) || rule.Role.Like(role);
                                if (!match) { Trace.WriteLine($"Rule role: {rule.Role} doesn't match specified role: {role}"); }
                                return match;
                            }); // short circuits when a matching rule is found.
                                // Returns false if no roles match those defined
                                // in rules for the specified context.
                        if (!result)
                        {
                            Trace.WriteLine($"Searching for rules for the given context: {Context} and roles: {String.Join(", ", Roles)} yields no results");
                            // TODO LOGME (Sean) $"Searching for rules for the given context: {Context} and roles: {String.Join(', ', Roles)} yields no results"
                        }
                        return result;
                    })?.Any(rule => {
                        var testValue = connType.GetProperties().FirstOrDefault(p => p.Name.Like(rule.ModelProperty))?.GetValue(Model).ToString();
                        // bail out if there's no property with that name on the value
                        if (testValue == null) return false;

                        if (rule.ModelValues != null && rule.ModelValues.Count() > 0)
                        {
                            return rule.ModelValues.Any(pattern => testValue.Like(pattern));   // Uses Like extension method that supports wildcards
                                                                                               // so you could say role: GarageAdmins could see all
                                                                                               // connectors that match: garage.* .
                        }
                        else
                        {
                            var pattern = rule.ModelValue;
                            return testValue.Like(pattern);
                        }
                    });
        }

        /// <summary>
        /// IEnumerable version of IsAuthorized&lt;T&gt;. Allows filtering a collection
        /// via extension method to make chaining with Linq methods more natural.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Collection"></param>
        /// <param name="Context"></param>
        /// <param name="Roles"></param>
        /// <returns></returns>
        public static IEnumerable<T> IsAuthorized<T>(this IEnumerable<T> Collection, string Context, IEnumerable<string> Roles)
        {
            return Collection.Where(x => IsAuthorized(x, Context, Roles));
        }


        /// <summary>
        /// For a given Context, an list of Roles, retrieve any matching rules and evaluate them
        /// against the Model. For AADC controllers it would be obvious
        /// to check if there is a rule that specifies a role that should allow access to
        /// connectors with a given name. The rule would for admins accessing my lab connector
        /// would look like this:
        ///   Role: "Admin"
        ///   ModelProperty: "Name"
        ///   ModelValue: "garage.mcardletech.com"  // actual example used in my lab not shilling my website,
        ///                                         // it's truly not worth your time.
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="Context"></param>
        /// <param name="Roles"></param>
        /// <returns></returns>
        public static bool IsAuthorized(Dictionary<string, object> Model, string Context, IEnumerable<string> Roles)
        {
            var rules = RegisteredRoleControllerRules.GetRoleControllerModelsByContext(Context);

            return (bool)rules.Where(
                    rule => {
                        bool result = (bool)Roles?.Any(
                            role => {
                                var match = role.Like(rule.Role) || rule.Role.Like(role);
                                if (!match) { Trace.WriteLine($"Rule role: {rule.Role} doesn't match specified role: {role}"); }
                                return match;
                            }); // short circuits when a matching rule is found.
                                // Returns false if no roles match those defined
                                // in rules for the specified context.
                        if (!result)
                        {
                            Trace.WriteLine($"Searching for rules for the given context: {Context} and roles: {String.Join(", ", Roles)} yields no results");
                            // TODO LOGME (Sean) $"Searching for rules for the given context: {Context} and roles: {String.Join(', ', Roles)} yields no results"
                        }
                        return result;
                    })?.Any(rule => {
                        object testValue = default(object);
                        var gotValue = Model.TryGetValue(rule.ModelProperty, out testValue);

                        // bail out if there's no value with that key
                        if (!gotValue) return false;

                        if (rule.ModelValues != null && rule.ModelValues.Count() > 0)
                        {
                            return rule.ModelValues.Any(pattern => (bool) testValue?.ToString().Like(pattern));   // Uses Like extension method that supports wildcards
                                                                                               // so you could say role: GarageAdmins could see all
                                                                                               // connectors that match: garage.* .
                        }
                        else
                        {
                            var pattern = rule.ModelValue;
                            return (bool) testValue?.ToString().Like(pattern);
                        }
                    });
        }

        /// <summary>
        /// IEnumerable version of IsAuthorized. Allows filtering a collection
        /// via extension method to make chaining with Linq methods more natural.
        /// </summary>
        /// <param name="Collection"></param>
        /// <param name="Context"></param>
        /// <param name="Roles"></param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, object>> IsAuthorized(this IEnumerable<Dictionary<string, object>> Collection, string Context, IEnumerable<string> Roles)
        {
            return Collection.Where(x => IsAuthorized(x, Context, Roles));
        }
    }
}