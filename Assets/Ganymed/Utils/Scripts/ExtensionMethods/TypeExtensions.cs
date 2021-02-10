using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Ganymed.Utils.ExtensionMethods
{
    /// <summary>
    /// Class containing Type Extension Methods
    /// </summary>
    public static class TypeExtensions
    {
        #region --- [DEFAULT] ---
        
        /// <summary>
        /// Returns an object with a default value for the given type.
        /// If the type is a string the method will return an empty string instead of null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static object TryGetDefaultInstance(this Type type)
        {
            // If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
            if (type == null || type == typeof(void))
                return null;
            
            if (type.IsEnum)
            {
                foreach (var enums in Enum.GetValues(type)) {
                    return enums;
                }
            }

            if (type == typeof(string))
            {
                return string.Empty;
            }
            
            // If the supplied Type has generic parameters, its default value cannot be determined
            if (type.ContainsGenericParameters)
                throw new ArgumentException(
                    "{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
                    "> contains generic parameters, so the default value cannot be retrieved");

            // If the type is of type string return an empty string 
            
            if (type.IsClass)
            {
                try
                {
                    return type.IsAssignableFrom(typeof(MonoBehaviour)) || type.IsSubclassOf(typeof(MonoBehaviour))
                        ? null
                        : Activator.CreateInstance(type);
                }
                catch
                {
                    return null;
                }
            }
            
            if (type.IsArray)
            {
                return type.GetArrayRank() > 1 ? Array.CreateInstance(type.GetElementType() ?? throw new Exception(), new int[type.GetArrayRank()]) : Array.CreateInstance(type.GetElementType()?? throw new Exception(), 0);
            }
            
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                // check if an empty array is an instance of T
                if (type.IsAssignableFrom(typeof(object[])))
                {
                    return new object[0];
                }

                if (type.IsGenericType && type.GetGenericArguments().Length == 1)
                {
                    var elementType = type.GetGenericArguments()[0];
                    if (type.IsAssignableFrom(elementType.MakeArrayType()))
                    {
                        return Array.CreateInstance(elementType, 0);
                    }
                }
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

        #region --- [TYPEOF] ---

        /// <summary>
        /// True if the type is a string.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsString(this Type type)
            => type == typeof(string);

        
        /// <summary>
        /// True if the type is a struct.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsStruct(this Type type)
            => type.IsValueType && !type.IsEnum && !type.IsPrimitive;
        
        
        /// <summary>
        /// True if the type is a Vector2 or Vector3 or Vector4.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsVector(this Type type) 
            => (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4));
        
        
        /// <summary>
        /// True if the type is a Vector2Int or Vector3Int.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsVectorInt(this Type type) 
            => (type == typeof(Vector2Int) || type == typeof(Vector3Int));
        
        
        /// <summary>
        /// True if the type is a Color or Color32.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsColor(this Type type)
            => (type == typeof(Color) || type == typeof(Color32));
        
        
        /// <summary>
        /// True if the type is a delegate.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsDelegate(this Type type)
            => typeof (MulticastDelegate).IsAssignableFrom(type.BaseType);
        
        
        /// <summary>
        /// Returns affiliation flags for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
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

        #endregion

        #region --- [UNDERLYING] ---

        /// <summary>
        /// Returns the underlying type.
        /// </summary>
        /// <param name="nullableType"></param>
        /// <returns></returns>
        public static Type GetNullableUnderlying(this Type nullableType)
            => Nullable.GetUnderlyingType(nullableType) ?? nullableType;
        

        #endregion
    }
}