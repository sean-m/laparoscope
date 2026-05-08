using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Reflection;

namespace SMM.Helper
{
    /// <summary>
    /// Provides intelligent type binding and property copying between different object types,
    /// with special handling for PowerShell objects and optimized POCO-to-POCO conversions.
    /// </summary>
    public static class TypeBinder
    {
        #region Public API

        /// <summary>
        /// Dynamically copies values from source to target type, selecting the appropriate
        /// conversion strategy at runtime based on the actual types.
        /// </summary>
        public static dynamic CopyValue(object source, Type targetType)
        {
            if (source == null) return null;
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));

            var sourceType = source.GetType();

            // Handle PSObject unwrapping
            if (source is PSObject psObj)
            {
                return CopyValueFromPSObject(psObj, targetType);
            }

            // Handle collections
            if (IsCollection(sourceType) && !IsString(sourceType))
            {
                return CopyCollection(source, targetType);
            }

            // Handle POCO-to-POCO
            if (IsComplexType(sourceType) && IsComplexType(targetType))
            {
                return CopyValuePOCO(source, sourceType, targetType);
            }

            // Handle simple types (primitives, strings, etc.)
            return ConvertSimpleType(source, targetType);
        }

        /// <summary>
        /// Copies values from a PSObject to a target type T.
        /// Unwraps the PSObject and maps properties by name (case-insensitive).
        /// </summary>
        public static T CopyValue<T>(PSObject source)
        {
            if (source == null) return default(T);
            return (T)CopyValueFromPSObject(source, typeof(T));
        }

        /// <summary>
        /// Copies values from one POCO to another POCO of the same type.
        /// </summary>
        public static T CopyValue<T>(T source)
        {
            if (source == null) return default(T);

            // For same-type copying, just return the source (or clone if needed)
            return source;
        }

        /// <summary>
        /// Copies values from one POCO type to another POCO type.
        /// Uses Transmute for performance when possible, falls back to reflection.
        /// </summary>
        public static TTarget CopyValue<TSource, TTarget>(TSource source)
        {
            if (source == null) return default(TTarget);

            return (TTarget)CopyValuePOCO(source, typeof(TSource), typeof(TTarget));
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Copies values from a PSObject to a target type using property name matching.
        /// </summary>
        private static object CopyValueFromPSObject(PSObject source, Type targetType)
        {
            if (source == null) return null;

            // Handle bool special case
            if (targetType == typeof(bool))
            {
                return Convert.ToBoolean(source.BaseObject);
            }

            // Handle simple types - unwrap from PSObject
            if (IsSimpleType(targetType))
            {
                return ConvertSimpleType(source.BaseObject, targetType);
            }

            // Create target instance
            object result;
            try
            {
                result = Activator.CreateInstance(targetType);
            }
            catch
            {
                // Can't create instance, return null
                return null;
            }

            // Map properties by name (case-insensitive)
            var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            foreach (var targetProp in targetProperties)
            {
                try
                {
                    var sourceProp = source.Properties
                        .FirstOrDefault(p => p.Name.Equals(targetProp.Name, StringComparison.OrdinalIgnoreCase));

                    if (sourceProp == null || sourceProp.Value == null)
                        continue;

                    var sourceValue = sourceProp.Value;

                    // Recursively copy the value using the appropriate method
                    var convertedValue = CopyValue(sourceValue, targetProp.PropertyType);

                    if (convertedValue != null || !targetProp.PropertyType.IsValueType)
                    {
                        targetProp.SetValue(result, convertedValue);
                    }
                }
                catch
                {
                    // Skip properties that fail to convert
                    // TODO log these conversion failures for better diagnostics
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// Copies values between two POCO types using the most efficient method available.
        /// Tries Transmute first for performance, falls back to reflection.
        /// </summary>
        private static object CopyValuePOCO(object source, Type sourceType, Type targetType)
        {
            if (source == null) return null;

            // Same type or directly assignable - no conversion needed
            if (targetType.IsAssignableFrom(sourceType))
            {
                return source;
            }

            // Try Transmute for performance
            if (TryTransmute(source, sourceType, targetType, out object transmuted))
            {
                return transmuted;
            }

            // Fall back to reflection-based property mapping
            return CopyValueReflection(source, sourceType, targetType);
        }

        /// <summary>
        /// Copies values using reflection-based property name matching.
        /// </summary>
        private static object CopyValueReflection(object source, Type sourceType, Type targetType)
        {
            if (source == null) return null;

            // Create target instance
            object result;
            try
            {
                result = Activator.CreateInstance(targetType);
            }
            catch
            {
                return null;
            }

            // Get properties from both types
            var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            var targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite)
                .ToList();

            foreach (var targetProp in targetProperties)
            {
                try
                {
                    if (!sourceProperties.TryGetValue(targetProp.Name, out PropertyInfo sourceProp))
                        continue;

                    var sourceValue = sourceProp.GetValue(source);
                    if (sourceValue == null)
                    {
                        if (!targetProp.PropertyType.IsValueType)
                            targetProp.SetValue(result, null);
                        continue;
                    }

                    // Handle Guid to string conversion
                    if (sourceValue is Guid guid && targetProp.PropertyType == typeof(string))
                    {
                        targetProp.SetValue(result, guid.ToString());
                        continue;
                    }

                    // Recursively copy complex values
                    var convertedValue = CopyValue(sourceValue, targetProp.PropertyType);

                    if (convertedValue != null || !targetProp.PropertyType.IsValueType)
                    {
                        targetProp.SetValue(result, convertedValue);
                    }
                }
                catch
                {
                    // Skip properties that fail to convert
                    continue;
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a collection from source to target type, converting each element.
        /// </summary>
        private static object CopyCollection(object source, Type targetType)
        {
            if (source == null) return null;
            if (!(source is IEnumerable sourceEnumerable)) return null;

            // Determine target element type
            Type targetElementType = GetElementType(targetType);
            if (targetElementType == null)
            {
                // Can't determine element type, return as-is
                return source;
            }

            // Create temporary list to hold converted items
            var tempListType = typeof(List<>).MakeGenericType(targetElementType);
            var tempList = (IList)Activator.CreateInstance(tempListType);

            // Convert each item
            foreach (var item in sourceEnumerable)
            {
                if (item == null)
                {
                    tempList.Add(null);
                    continue;
                }

                try
                {
                    var convertedItem = CopyValue(item, targetElementType);
                    tempList.Add(convertedItem);
                }
                catch
                {
                    // If conversion fails, add null to prevent type mismatch
                    tempList.Add(null);
                }
            }

            // Convert to target type (array or list)
            if (targetType.IsArray)
            {
                var array = Array.CreateInstance(targetElementType, tempList.Count);
                tempList.CopyTo(array, 0);
                return array;
            }

            return tempList;
        }

        /// <summary>
        /// Attempts POCO-to-POCO conversion using DynamicTypeRegistry.Transmute for performance.
        /// </summary>
        private static bool TryTransmute(object source, Type sourceType, Type targetType, out object result)
        {
            result = null;

            try
            {
                // Don't transmute if types are the same or directly assignable
                if (targetType.IsAssignableFrom(sourceType))
                {
                    result = source;
                    return true;
                }

                // Don't try to transmute primitive types or strings
                if (IsSimpleType(sourceType) || IsSimpleType(targetType))
                {
                    return false;
                }

                var transmuteMethod = typeof(DynamicTypeRegistry).GetMethod(nameof(DynamicTypeRegistry.Transmute));
                if (transmuteMethod == null) return false;

                var genericTransmute = transmuteMethod.MakeGenericMethod(sourceType, targetType);
                var transmuteExpression = genericTransmute.Invoke(null, null);
                var compiledTransmute = transmuteExpression.GetType()
                    .GetMethod("Compile", Type.EmptyTypes)
                    .Invoke(transmuteExpression, null);

                result = ((Delegate)compiledTransmute).DynamicInvoke(source);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts simple types (primitives, strings, Guid, DateTime, etc.)
        /// </summary>
        private static object ConvertSimpleType(object source, Type targetType)
        {
            if (source == null) return null;

            var sourceType = source.GetType();

            // Already the right type
            if (targetType.IsAssignableFrom(sourceType))
                return source;

            // Guid to string
            if (source is Guid guid && targetType == typeof(string))
                return guid.ToString();

            // String conversions
            if (targetType == typeof(string))
                return source.ToString();

            // Enum conversions
            if (targetType.IsEnum)
            {
                if (sourceType == typeof(string))
                    return Enum.Parse(targetType, (string)source, true);
                return Enum.ToObject(targetType, source);
            }

            // Try Convert.ChangeType for primitives
            try
            {
                return Convert.ChangeType(source, targetType);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Type Helpers

        private static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid);
        }

        private static bool IsComplexType(Type type)
        {
            return !IsSimpleType(type) && !IsCollection(type) && type != typeof(object);
        }

        private static bool IsCollection(Type type)
        {
            if (type == typeof(string)) return false;
            return typeof(IEnumerable).IsAssignableFrom(type);
        }

        private static bool IsString(Type type)
        {
            return type == typeof(string);
        }

        private static Type GetElementType(Type type)
        {
            // Array type
            if (type.IsArray)
                return type.GetElementType();

            // Generic collection type
            if (type.IsGenericType)
            {
                var genericArgs = type.GetGenericArguments();
                if (genericArgs.Length == 1)
                    return genericArgs[0];
            }

            // IEnumerable<T>
            var enumerableInterface = type.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (enumerableInterface != null)
                return enumerableInterface.GetGenericArguments()[0];

            return null;
        }

        #endregion
    }
}
