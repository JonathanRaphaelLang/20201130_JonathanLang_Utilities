using System;
using System.Collections.Generic;
using System.Reflection;
using Ganymed.Utils.Attributes;
using Ganymed.Utils.ColorTables;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Utils.Editor.AttributeValidation
{
    internal static partial class AttributeReflection
    {
        private static void ReflectValidBindingFlagsAttribute(IEnumerable<MemberInfo> memberInfos)
        {
            foreach (var memberInfo in memberInfos)

                switch (memberInfo)
                {
                    case MethodInfo methodInfo:
                        foreach (var inspected in methodInfo.GetBaseDefinition().GetCustomAttributes())
                        foreach (var attribute in inspected.GetType().GetCustomAttributes())
                            if (attribute is RequiredAccessAttribute validBindingFlagsAttribute)
                            {
                                string requiredPublic = null;
                                string requiredStatic = null;

                                if (validBindingFlagsAttribute.PublicRequiredOrNull != null)
                                {
                                    var _public = (bool) validBindingFlagsAttribute.PublicRequiredOrNull;
                                    if (_public != methodInfo.IsPublic)
                                        requiredPublic = $"{(_public ? "public" : "private")}";
                                }

                                if (validBindingFlagsAttribute.StaticRequiredOrNull != null)
                                {
                                    var _static = (bool) validBindingFlagsAttribute.StaticRequiredOrNull;
                                    if (_static != methodInfo.IsStatic)
                                        requiredStatic = $"{(_static ? "static" : "non static")}";
                                }

                                if (requiredPublic != null || requiredStatic != null)
                                    LogValidBindingFlagsWarning(
                                        AttributeTargets.Method,
                                        requiredPublic,
                                        requiredStatic,
                                        inspected,
                                        memberInfo);
                            }

                        break;


                    case FieldInfo fieldInfo:
                        foreach (var inspected in fieldInfo.GetCustomAttributes())
                        foreach (var attribute in inspected.GetType().GetCustomAttributes())
                            if (attribute is RequiredAccessAttribute validBindingFlagsAttribute)
                            {
                                string requiredPublic = null;
                                string requiredStatic = null;

                                if (validBindingFlagsAttribute.PublicRequiredOrNull != null)
                                {
                                    var _public = (bool) validBindingFlagsAttribute.PublicRequiredOrNull;
                                    if (_public != fieldInfo.IsPublic)
                                        requiredPublic = $"{(_public ? "public" : "private")}";
                                }

                                if (validBindingFlagsAttribute.StaticRequiredOrNull != null)
                                {
                                    var _static = (bool) validBindingFlagsAttribute.StaticRequiredOrNull;
                                    if (_static != fieldInfo.IsStatic)
                                        requiredStatic = $"{(_static ? "static" : "non static")}";
                                }

                                if (requiredPublic != null || requiredStatic != null)
                                    LogValidBindingFlagsWarning(
                                        AttributeTargets.Field,
                                        requiredPublic,
                                        requiredStatic,
                                        inspected,
                                        memberInfo);
                            }

                        break;


                    case PropertyInfo propertyInfo:
                        foreach (var inspected in propertyInfo.GetBaseDefinition().GetCustomAttributes())
                        foreach (var attribute in inspected.GetType().GetCustomAttributes())
                            if (attribute is RequiredAccessAttribute validBindingFlagsAttribute)
                            {
                                string requiredPublic = null;
                                string requiredStatic = null;

                                if (validBindingFlagsAttribute.PublicRequiredOrNull != null)
                                {
                                    var _public = (bool) validBindingFlagsAttribute.PublicRequiredOrNull;
                                    if (_public != propertyInfo.GetAccessors(true)[0].IsPublic)
                                        requiredPublic = $"{(_public ? "public" : "private")}";
                                }

                                if (validBindingFlagsAttribute.StaticRequiredOrNull != null)
                                {
                                    var _static = (bool) validBindingFlagsAttribute.StaticRequiredOrNull;
                                    if (_static != propertyInfo.GetAccessors(true)[0].IsStatic)
                                        requiredStatic = $"{(_static ? "static" : "non static")}";
                                }

                                if (requiredPublic != null || requiredStatic != null)
                                    LogValidBindingFlagsWarning(
                                        AttributeTargets.Property,
                                        requiredPublic,
                                        requiredStatic,
                                        inspected,
                                        memberInfo);
                            }

                        break;


                    case ConstructorInfo constructorInfo:
                        foreach (var inspected in constructorInfo.GetCustomAttributes())
                        foreach (var attribute in inspected.GetType().GetCustomAttributes())
                            if (attribute is RequiredAccessAttribute validBindingFlagsAttribute)
                            {
                                string requiredPublic = null;
                                string requiredStatic = null;

                                if (validBindingFlagsAttribute.PublicRequiredOrNull != null)
                                {
                                    var _public = (bool) validBindingFlagsAttribute.PublicRequiredOrNull;
                                    if (_public != constructorInfo.IsPublic)
                                        requiredPublic = $"{(_public ? "public" : "private")}";
                                }

                                if (validBindingFlagsAttribute.StaticRequiredOrNull != null)
                                {
                                    var _static = (bool) validBindingFlagsAttribute.StaticRequiredOrNull;
                                    if (_static != constructorInfo.IsStatic)
                                        requiredStatic = $"{(_static ? "static" : "non static")}";
                                }

                                if (requiredPublic != null || requiredStatic != null)
                                    LogValidBindingFlagsWarning(
                                        AttributeTargets.Constructor,
                                        requiredPublic,
                                        requiredStatic,
                                        inspected,
                                        memberInfo);
                            }

                        break;
                }
        }

        private static void LogValidBindingFlagsWarning(
            AttributeTargets target,
            [CanBeNull] string requiredPublic,
            [CanBeNull] string requiredStatic,
            Attribute origin,
            MemberInfo memberInfo)
        {
            var message =
                $"Warning: the {CS.Violet}{target}{CS.Clear} " +
                $"'{CS.Orange}{memberInfo.Name}{CS.Clear}' in: " +
                $"{CS.LightGray}{memberInfo.DeclaringType?.FullName}.{CS.Clear}" +
                $"{CS.Blue}{memberInfo.Name}{CS.Clear} requires: " +
                $"{(requiredPublic != null ? $"{CS.Red}'{requiredPublic}'{CS.Clear}" : "")}" +
                $"{(requiredPublic != null && requiredStatic != null ? " and " : "")}" +
                $"{(requiredStatic != null ? $"{CS.Red}'{requiredStatic}'{CS.Clear}" : "")} access " +
                $"because of the members '{CS.Orange}{origin.GetType().Name.Delete("Attribute")}{CS.Clear}' Attribute.";

            Debug.LogWarning(message);
        }
    }
}