using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ganymed.Utils.Attributes;
using Ganymed.Utils.ColorTables;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Utils.Editor.AttributeValidation
{
    internal static partial class AttributeReflection
    {
        /// <summary>
        /// Method is reflection over Methods/Constructor Parameter...
        /// </summary>
        /// <param name="methodInfos"></param>
        private static void ReflectTargetParameterTypeRestrictions(IEnumerable<MethodInfo> methodInfos)
        {
            #region --- [METHODS] ---

            foreach (var methodInfo in methodInfos)
            {
                foreach (var inspected in methodInfo.GetBaseDefinition().GetCustomAttributes())
                {
                    if (methodInfo.GetCustomAttributes(true).OfType<AllowUnsafeAttribute>().Any()) continue;
                    
                    var typeRestrictionsAttributes =
                        inspected.GetType().GetCustomAttributes().OfType<TargetParamRestrictionsAttribute>().ToArray();
                    
                    if (typeRestrictionsAttributes.Length <= 0) continue;
                    {
                        var viableAffiliations = typeRestrictionsAttributes.Aggregate(TypeAffiliations.None,
                            (current, propertyAttribute) => current | propertyAttribute.ValidTypeAffiliations);
                        
                        var viableTypes = new List<Type>();
                        var viableInheritingTypes = new List<Type>();
                        
                        foreach (var attribute in typeRestrictionsAttributes)
                        {
                            if (!attribute.Inherited)
                            {
                                foreach (var type in attribute.ValidTypes)
                                {
                                    if (!viableTypes.Contains(type))
                                        viableTypes.Add(type);
                                }
                            }
                            else
                            {
                                foreach (var type in attribute.ValidTypes)
                                {
                                    if (!viableInheritingTypes.Contains(type))
                                        viableInheritingTypes.Add(type);
                                    if (!viableInheritingTypes.Contains(type))
                                        viableInheritingTypes.Add(type);
                                }
                            }
                        }

                        foreach (var parameterInfo in methodInfo.GetParameters())
                        {
                            if (viableAffiliations.HasFlag(parameterInfo.ParameterType.GetTypeAffiliation())) continue;
                            if (viableTypes.Contains(parameterInfo.ParameterType)) continue;
                            if (viableInheritingTypes.Any(type =>
                                parameterInfo.ParameterType.IsSubclassOf(type) || parameterInfo.ParameterType.IsAssignableFrom(type))) continue;

                            LogTargetParameterTypeRestrictionsWarningMessage(
                                viableAffiliations,
                                viableInheritingTypes,
                                viableTypes,
                                methodInfo,
                                parameterInfo,
                                inspected);
                        }
                    }
                }
            }
            #endregion
        }
        
        private static void LogTargetParameterTypeRestrictionsWarningMessage(
            TypeAffiliations allowedAffiliations,
            IReadOnlyCollection<Type> allowedInheritingTypes,
            IReadOnlyCollection<Type> allowedTypes,
            MethodBase methodBase,
            ParameterInfo parameterInfo,
            Attribute inspected)
        {
            var warning =
                $"Parameter '{CS.Blue}{parameterInfo.Name}{CS.Clear} " +
                $"of Method '{CS.LightGray}{methodBase.DeclaringType?.Namespace}{CS.Clear}." +
                $"{CS.Blue}{methodBase.Name}{CS.Clear}' " +
                $"has invalid type: {CS.Red}'{parameterInfo.ParameterType}'.{CS.Clear}" +
                $"\nAllowed types for methods with the attribute " +
                $"[{CS.Blue}{inspected.GetType().Name.Delete("Attribute")}{CS.Clear}] are: " +
                            
                $"{(allowedInheritingTypes.Count > 0? $"Inheriting types: [{CS.Blue}" : "")}" +
                $"{string.Join(" ", allowedInheritingTypes.Select(x => $"{x.Name}, ")).RemoveFormEnd(2)}" +
                $"{(allowedInheritingTypes.Count > 0? $"{CS.Clear}] " : "")}" +
                            
                $"{(allowedTypes.Count > 0? $"Types: [{CS.Blue}" : "")}" +
                $"{string.Join(" ", allowedTypes.Select(x => $"{x.Name}, ")).RemoveFormEnd(2)}" +
                $"{(allowedTypes.Count > 0? $"{CS.Clear}] " : "")}" +
                            
                $"{(allowedAffiliations != TypeAffiliations.None? $"Affiliations: [{CS.Blue}" : "")}" +
                $"{allowedAffiliations}" +
                $"{(allowedAffiliations != TypeAffiliations.None? $"{CS.Clear}] " : "")}" +
                            
                $"You should either validate the type of the parameter or use the " +
                $"{CS.Orange}" +
                $"{nameof(AllowUnsafeAttribute).Replace(nameof(Attribute), "")}{CS.Clear} " +
                $"attribute to suppress this Warning";

            Debug.LogWarning(warning);
        }
        
        
    }
}