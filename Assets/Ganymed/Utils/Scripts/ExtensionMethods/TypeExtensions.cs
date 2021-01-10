﻿using System;
using System.Reflection;
using UnityEngine;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class TypeExtensions
    {
        #region --- [DEFAULT]

        /// <summary>
        /// [ <c>public static object GetDefault(this Type type)</c> ]
        /// <para></para>
        /// Retrieves the default value for a given Type
        /// </summary>
        /// <param name="type">The Type for which to get the default value</param>
        /// <returns>The default value for <paramref name="type"/></returns>
        /// <remarks>
        /// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type 
        /// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an 
        /// exception.
        /// </remarks>
        /// <example>
        /// To use this method in its native, non-extension form, make a call like:
        /// <code>
        ///     object Default = DefaultValue.GetDefault(someType);
        /// </code>
        /// To use this method in its Type-extension form, make a call like:
        /// <code>
        ///     object Default = someType.GetDefault();
        /// </code>
        /// </example>
        /// <seealso cref="GetDefault"/>
        public static object GetDefault(this Type type)
        {
            // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
            if (type == null || !type.IsValueType || type == typeof(void))
                return null;
            
            if (type.IsEnum)
            {
                foreach (var enums in Enum.GetValues(type)) {
                    return enums;
                }
            }

            // If the supplied Type has generic parameters, its default value cannot be determined
            if (type.ContainsGenericParameters)
                throw new ArgumentException(
                    "{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                    "> contains generic parameters, so the default value cannot be retrieved");

            // If the type is of type string return an empty string 
            if (type == typeof(string))
            {
                return string.Empty;
            }

            // If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
            //  default instance of the value type
            if (type.IsPrimitive || !type.IsNotPublic)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception e)
                {
                    throw new ArgumentException(
                        "{" + MethodBase.GetCurrentMethod() +
                        "} Error:\n\nThe Activator.CreateInstance method could not " +
                        "create a default instance of the supplied value type <" + type +
                        "> (Inner Exception message: \"" + e.Message + "\")", e);
                }
            }

            // Fail with exception
            throw new ArgumentException("{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" +
                                        type +
                                        "> is not a publicly-visible type, so the default value cannot be retrieved");
        }
        #endregion
        
        public static bool IsString(this Type type) => type == typeof(string);

        public static bool IsStruct(this Type type)
            => type.IsValueType && !type.IsEnum && !type.IsPrimitive;

        public static bool IsVector(this Type type) 
            => (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4));
        
        public static bool IsVectorInt(this Type type) 
            => (type == typeof(Vector2Int) || type == typeof(Vector3Int));
        
        public static bool IsColor(this Type type)
            => (type == typeof(Color) || type == typeof(Color32));
        
        public static bool IsDelegate(this Type type)
            => typeof (MulticastDelegate).IsAssignableFrom(type.BaseType);
        
        public static Type GetNullableUnderlying(this Type nullableType)
            => Nullable.GetUnderlyingType(nullableType) ?? nullableType;
        
        
        public static TypeAffiliations GetTypeAffiliation(this Type type)
        {
            return
                type.IsEnum ? TypeAffiliations.Enum :
                type.IsStruct() ? TypeAffiliations.Struct :
                type.IsString() ? TypeAffiliations.String :
                type.IsInterface ? TypeAffiliations.Primitive :
                type.IsGenericType ? TypeAffiliations.Generic :
                type.IsInterface ? TypeAffiliations.Interface :
                type.IsClass ? TypeAffiliations.Class : TypeAffiliations.None;
        }
    }
}