using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ganymed.Utils.Attributes;
using Ganymed.Utils.ColorTables;
using Ganymed.Utils.MessageCodes;
using UnityEngine;

namespace Ganymed.Utils.Editor.AttributeValidation
{
    internal static partial class AttributeReflection
    {
        /// <summary>
        /// Method checks integrity of 'PropertyAccessRequirementAttribute' instances present within the passes propertyInfos.
        /// </summary>
        /// <param name="propertyInfos"></param>
        private static void ReflectPropertyAccessRequirement(IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (var propertyInfo in propertyInfos)
            {
                foreach (var attribute in propertyInfo.GetCustomAttributes(true))
                {
                    var attributes = attribute.GetType().GetCustomAttributes(true).OfType<PropertyAccessRequirementAttribute>();
                    
                    foreach (var propertyRestrictionAttribute in attributes)
                    {
                        var canRead = propertyInfo.CanRead;
                        var canWrite = propertyInfo.CanWrite;
                        
                        // --- if a getter && setter are required
                        if (propertyRestrictionAttribute.RequiresReadAndWrite && !canRead && !canWrite)
                        {
                            var message =
                                $"Warning: the {CS.Violet}Property{CS.Clear} " +
                                $"'{CS.Orange}{propertyInfo.Name}{CS.Clear}' in " +
                                $"'{CS.LightGray}{propertyInfo.DeclaringType?.Namespace}.{CS.Clear}{CS.Blue}{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}'{CS.Clear} " +
                                $"requires {CS.Red}read and write accessibility. {CS.Clear}(Getter/Setter)";
                            Debug.LogWarning(message);
                        }
                        // --- if a getter is required
                        if (propertyRestrictionAttribute.RequiresWrite && !canWrite)
                        {
                            var message =
                                $"Warning: the {CS.Violet}Property{CS.Clear} " +
                                $"'{CS.Orange}{propertyInfo.Name}{CS.Clear}' in " +
                                $"'{CS.LightGray}{propertyInfo.DeclaringType?.Namespace}.{CS.Clear}{CS.Blue}{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}'{CS.Clear} " +
                                $"requires {CS.Red}write accessibility. {CS.Clear}(Setter)";
                            Debug.LogWarning(message);
                        }
                        // --- if a setter is required
                        if (propertyRestrictionAttribute.RequiresRead && !canRead)
                        {
                            var message =
                                $"Warning: the {CS.Violet}Property{CS.Clear} " +
                                $"'{CS.Orange}{propertyInfo.Name}{CS.Clear}' in " +
                                $"'{CS.LightGray}{propertyInfo.DeclaringType?.Namespace}.{CS.Clear}{CS.Blue}{propertyInfo.DeclaringType?.Name}.{propertyInfo.Name}'{CS.Clear} " +
                                $"requires {CS.Red}read accessibility. {CS.Clear}(Getter)";
                            Debug.LogWarning(message);
                        }
                    }
                }
            }
        }
    }
}