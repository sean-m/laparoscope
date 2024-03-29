﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.DirectoryServices;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SMM.Helper
{
    internal static partial class Extensions
    {
        /// <summary>
        /// VisualBasic's string comparison with wildcard support.
        /// </summary>
        /// <param name="Base">The value to check.</param>
        /// <param name="Pattern">The pattern compared to 'Base'. Supports simple wildcards: *, ?.
        /// </param>
        /// <returns></returns>
        public static bool Like(this string Base, string Pattern, bool CaseSensitive = false)
        {
            var pattern = WildCardToRegular(Pattern);
            var options = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;  // Case insensitive is the default.
            if (CaseSensitive) { options = options ^ RegexOptions.IgnoreCase; }
            return Regex.IsMatch(Base, pattern, options);
        }

        /// <summary>
        /// Decompose simple wildcard characters in to their regex equals.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static String WildCardToRegular(String value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
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
