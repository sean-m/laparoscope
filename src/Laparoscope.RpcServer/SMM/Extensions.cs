using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Threading;

namespace SMM.Helper
{

    /// <summary>
    /// A simple converter from Guid to string using Guid.ToString().
    /// </summary>
    public class GuidToStringConverter : IConverter<Guid, string> {
        public string Convert(Guid value)
        {
            return value.ToString();
        }
    }

    /// <summary>
    /// Interface for converting one type to another.
    /// </summary>
    public interface IConverter<TSource, TDestination> {
        TDestination Convert(TSource value);
    }

    public static class Extensions
    {
        #region PSObjectHandling
        /// <summary>
        /// Naivly unboxes a single PSObject into a Dictionary(string, object).
        /// If the PSObject is a string, it is stored as "Output": value.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDict(this PSObject input)
        {
            var result = new Dictionary<string, object>();
            if (input.TypeNames.Contains(typeof(string).FullName))
            {
                result.Add("Output", String.Copy(input.ToString()));
            }
            else
            {
                foreach (var p in input.Properties)
                {
                    result.Add(p.Name, p.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// Unboxes a collection of PSObjects, returning as a List(Dictionary(string, object)).
        /// The resulting value is not very ergonomic but serializes to JSON well.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> ToDict(this PSDataCollection<PSObject> input)
        {
            if (input == null) return null;

            var result = new List<Dictionary<string, object>>();
            foreach (var i in input)
            {
                result.Add(i.ToDict());
            }
            return result;
        }


        /// <summary>
        /// Attempts to map properties from the PSObject to properties from the
        /// desired result type using a case insensitive property name match.
        /// If successful, the dynamic result will be of the desired type.
        /// </summary>
        /// <typeparam name="T">The type to map to.</typeparam>
        /// <param name="input">The PSObject we're mapping values from.</param>
        /// <returns></returns>
        public static dynamic CapturePSResult<T>(this PSObject input)
        {
            dynamic result = default(dynamic);
            var asType = typeof(T);

            bool captureSuccess = true;
            Type lastSeenType;
            try {
                if (typeof(T) == typeof(bool))
                {
                    result = Convert.ToBoolean(input.BaseObject);
                    return result;
                }

                result = Activator.CreateInstance(asType);
                var hintType = asType.GetTypeInfo();
                var props = hintType.DeclaredProperties;
                foreach (var p in props)
                {
                    var psprop = input.Properties.FirstOrDefault(x => x.Name.Equals(p.Name, StringComparison.CurrentCultureIgnoreCase));
                    
                    if (psprop != null)
                    {
                        lastSeenType = psprop.Value?.GetType();
                        if (psprop.Value is PSObject psobj)
                        {
                            if (p.PropertyType == typeof(T))
                            {
                                p.SetValue(result, psobj.CapturePSResult<T>());
                            }
                            else { p.SetValue(result, psobj.ToDict()); }
                        }
                        else
                        {
                            object value = psprop.Value;
                            if (psprop.Value is Guid guid)
                            {
                                var guidstring = guid.ToString();
                                p.SetValue(result, guidstring);
                                continue;
                            }

                            p.SetValue(result, value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetAllMessages());
                captureSuccess = false;
            }

            // Capturing the PSObject as the desired type failed.
            // Fall back to returning a Dictionary<string, object>.
            if (!captureSuccess) result = input.ToDict();

            return result;
        }

        /// <summary>
        /// Loop through the collection and capture the PSObjects as the desired type T.
        /// If the mapping fails, results are returned as a Dictionary(string, object) so
        /// though the return type is dynamic, it should be checked with the as operator
        /// or GetType().
        /// </summary>
        /// <typeparam name="T">Desired result type.</typeparam>
        /// <param name="input">PSObject collection to capture values from.</param>
        /// <returns></returns>
        public static IEnumerable<dynamic> CapturePSResult<T>(this PSDataCollection<PSObject> input)
        {
            var result = new List<dynamic>();
            var asType = typeof(T);

            foreach (var i in input)
            {
                result.Add(i.CapturePSResult<T>());
            }

            return result;
        }
        #endregion  // PSObjectHandling

        public static bool TryCast<T>(object obj, out T result)
        {
            result = default(T);
            if (obj is T)
            {
                result = (T)obj;
                return true;
            }

            // If it's null, we can't get the type.
            if (obj != null)
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.CanConvertFrom(obj.GetType()))
                    result = (T)converter.ConvertFrom(obj);
                else
                    return false;

                return true;
            }

            //Be permissive if the object was null and the target is a ref-type
            return !typeof(T).IsValueType;
        }

        private readonly static object _lock = new object();

        public static T CloneObject<T>(T original)
        {
            try
            {
                Monitor.Enter(_lock);
                T copy = Activator.CreateInstance<T>();
                PropertyInfo[] piList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo pi in piList)
                {
                    if (pi.GetValue(copy, null) != pi.GetValue(original, null))
                    {
                        try
                        {
                            pi.SetValue(copy, pi.GetValue(original, null), null);
                        }
                        catch (Exception e) when (e.Message == "Property set method not found.")
                        {
                            // I don't care about not being able to set private properties
                        }
                    }
                }
                return copy;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static T CloneObject<T>(T original, List<string> propertyExcludeList)
        {
            try
            {
                Monitor.Enter(_lock);
                T copy = Activator.CreateInstance<T>();
                PropertyInfo[] piList = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (PropertyInfo pi in piList)
                {
                    if (!propertyExcludeList.Contains(pi.Name))
                    {
                        if (pi.GetValue(copy, null) != pi.GetValue(original, null))
                        {
                            pi.SetValue(copy, pi.GetValue(original, null), null);
                        }
                    }
                }
                return copy;
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        public static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static string GetAllMessages(this Exception exception)
        {
            var messages = exception.FromHierarchy(ex => ex.InnerException)
                .Select(ex => ex.Message);
            return String.Join(Environment.NewLine, messages);
        }
    }
}
