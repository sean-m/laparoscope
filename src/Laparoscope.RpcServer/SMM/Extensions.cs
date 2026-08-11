using Microsoft.Management.Infrastructure;
using Microsoft.PowerShell.Cim;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.CodeDom;
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

    public static partial class Extensions
    {
        /// <summary>
        /// VisualBasic's string comparison with wildcard support.
        /// </summary>
        /// <param name="Base">The value to check.</param>
        /// <param name="Pattern">The pattern compared to 'Base'. Supports wildcards
        /// and other niceties. More info: https://docs.microsoft.com/en-us/office/vba/Language/Reference/User-Interface-Help/wildcard-characters-used-in-string-comparisons.
        /// </param>
        /// <returns></returns>
        public static bool Like(this string Base, string Pattern)
        {
            return LikeOperator.LikeString(Base, Pattern, Microsoft.VisualBasic.CompareMethod.Text);
        }

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
    }
}
