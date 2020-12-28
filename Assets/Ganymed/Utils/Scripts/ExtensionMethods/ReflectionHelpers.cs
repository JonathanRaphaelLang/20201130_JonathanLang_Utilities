using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class ReflectionHelpers
    {
        #region --- [TYPE EXTENSIONS] ---

        public static Type[] GetAllDerivedTypes(this AppDomain aAppDomain, Type aType)
        {
            var result = new List<Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
            }

            return result.ToArray();
        }

        public static Type[] GetAllDerivedTypes<T>(this AppDomain aAppDomain)
        {
            return GetAllDerivedTypes(aAppDomain, typeof(T));
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [INTERFACE TYPE EXTENSIONS] ---

        public static System.Type[] GetTypesWithInterface(this System.AppDomain aAppDomain, System.Type aInterfaceType)
        {
            var result = new List<System.Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                    if (aInterfaceType.IsAssignableFrom(type))
                        result.Add(type);
            }

            return result.ToArray();
        }

        public static System.Type[] GetTypesWithInterface<T>(this System.AppDomain aAppDomain)
        {
            return GetTypesWithInterface(aAppDomain, typeof(T));
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [PARAMETER INFO] ---

        public static bool isParams(this ParameterInfo param)
        {
            return param.GetCustomAttributes(typeof (ParamArrayAttribute), false).Length > 0;
        }
        
        public static bool TryGetAttribute<T>(this ParameterInfo info, out T attribute) where T : Attribute
        {
            attribute = null;
            
            foreach (var viewed in info.GetCustomAttributes())
            {
                if (viewed.GetType() != typeof(T)) continue;
                attribute = (T)viewed;
                return true;
            }

            return false;
        }

        #endregion
    }
}