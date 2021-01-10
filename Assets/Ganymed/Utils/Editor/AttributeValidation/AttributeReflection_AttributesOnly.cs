using System;
using System.Collections.Generic;
using System.Reflection;
using Ganymed.Utils.Attributes;
using Ganymed.Utils.ColorTables;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Utils.Editor.AttributeValidation
{
    internal static partial class AttributeReflection
    {
        private static void ReflectAttributesOnly(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                foreach (var inspected in type.GetNullableUnderlying().GetCustomAttributes())
                {
                    foreach (var attribute in inspected.GetType().GetNullableUnderlying().GetCustomAttributes())
                    {
                        if (!(attribute is AttributesOnlyAttribute)) continue;
                        if (type.IsAssignableFrom(typeof(Attribute)) || type.IsSubclassOf(typeof(Attribute)))
                        {
                            continue;
                        }
                        Debug.LogWarning(
                            $"The {nameof(Attribute)} {CS.Orange}{inspected.GetType().Name}{CS.Clear} in " +
                            $"{CS.LightGray}{type.Namespace}{CS.Clear}.{CS.Blue}{type.Name}{CS.Clear} " +
                            $"is only a valid attribute for classes that are assignable from " +
                            $"or are a subclass of {nameof(Attribute)}");
                    }
                }
            }
        }
    }
}