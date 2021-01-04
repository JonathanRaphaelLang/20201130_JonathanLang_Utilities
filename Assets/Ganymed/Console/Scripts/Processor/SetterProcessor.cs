using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    public static class SetterProcessor
    {
        #region --- [FIELDS] ---
        
        private static readonly Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<string, string> FieldsCut = new Dictionary<string, string>();
        
        private static readonly Dictionary<string, PropertyInfo> Properties = new Dictionary<string, PropertyInfo>();
        private static readonly Dictionary<string, string> PropertiesCut = new Dictionary<string, string>();

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMAND] ---

        [NativeCommand]
        [Command(CommandProcessor.SetKey, 10000)]
        private static void SetCommand([CanBeNull] string target, object value)
        {
            if (target == null)
            {
                GetSetter();
            }
            
            else if (Fields.TryGetValue(target, out var fieldInfo))
            {
                try
                {
                    value = Convert.ChangeType(value, fieldInfo.FieldType);
                    fieldInfo.SetValue(null, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            else if (Properties.TryGetValue(target, out var propertyInfo))
            {
                try
                {
                    value = Convert.ChangeType(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(null, value);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
        
        private static void GetSetter()
        {
            Transmission.Start(TransmissionOptions.Enumeration, $"{nameof(CommandProcessor)} Setter");
            Transmission.AddTitle("Fields (Set)");
            Transmission.AddLine("Fields", "Type", "Abbreviation");
            Transmission.AddBreak();
            foreach (var setter in Fields)
            {
                if(FieldsCut.ContainsValue(setter.Key))
                {
                    Transmission.AddLine(setter.Key, setter.Value.FieldType, FieldsCut.FirstOrDefault(x => x.Value == setter.Key).Key);
                }
                else
                {
                    Transmission.AddLine(setter.Key, setter.Value.FieldType);    
                }
            }
            Transmission.AddBreak();
            Transmission.AddTitle("Properties (Set)");
            Transmission.AddLine("Property", "Type", "Abbreviation");
            Transmission.AddBreak();
            foreach (var setter in Properties)
            {
                if(PropertiesCut.ContainsValue(setter.Key))
                {
                    Transmission.AddLine(setter.Key, setter.Value.PropertyType, PropertiesCut.FirstOrDefault(x => x.Value == setter.Key).Key);
                }
                else
                {
                    Transmission.AddLine(setter.Key, setter.Value.PropertyType);
                }
            }
            Transmission.ReleaseAsync();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        internal static void FindSetterInAssembly()
        {
            var types =  AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(object));

            foreach (var type in types)
            {
                foreach (var fieldInfo in type.GetFields(CommandProcessor.SetterFieldFlags))
                {
                    var native = fieldInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                    
                    if ((fieldInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute attribute))
                    {
                        if (fieldInfo.DeclaringType is null) continue;
                        var key = native? $"{fieldInfo.Name}" : $"{fieldInfo.DeclaringType.Name}.{fieldInfo.Name}";
                        Fields.Add(key, fieldInfo);
                        if(attribute.Shortcut != null) FieldsCut.Add(attribute.Shortcut, key);
                    }
                    else if ((fieldInfo.GetCustomAttribute(typeof(SetterAttribute)) is SetterAttribute setterAttribute))
                    {
                        if (fieldInfo.DeclaringType is null) continue;
                        var key = native? $"{fieldInfo.Name}" : $"{fieldInfo.DeclaringType.Name}.{fieldInfo.Name}";
                        Fields.Add(key, fieldInfo);
                        if(setterAttribute.Shortcut != null) FieldsCut.Add(setterAttribute.Shortcut, key);
                    }
                }
                foreach (var propertyInfo in type.GetProperties(CommandProcessor.SetterPropertyFlags))
                {
                    var native = propertyInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                    
                    if ((propertyInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute attribute))
                    {
                        if (propertyInfo.DeclaringType is null) continue;
                        var key = native? $"{propertyInfo.Name}" : $"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name}";
                        Properties.Add(key, propertyInfo);
                        if(attribute.Shortcut != null) PropertiesCut.Add(attribute.Shortcut, key);
                    }
                    else if ((propertyInfo.GetCustomAttribute(typeof(SetterAttribute)) is SetterAttribute setterAttribute))
                    {
                        if (propertyInfo.DeclaringType is null) continue;
                        var key = native? $"{propertyInfo.Name}" : $"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name}";
                        Properties.Add(key, propertyInfo);
                        if(setterAttribute.Shortcut != null) PropertiesCut.Add(setterAttribute.Shortcut, key);
                    }
                }
            }
        }

        #endregion
    
        //--------------------------------------------------------------------------------------------------------------

        #region --- [PROCESS] ---

        internal static void Process()
        {
        
        }

        #endregion

        #region --- [PROPOSE] ---

        #region --- [PROPOSE] ---

        internal static bool Propose(string input, out string proposedDescription, out string proposedInput, out InputValidation inputValidation)
        {
            proposedDescription = string.Empty;
            proposedInput  = string.Empty;
            inputValidation = InputValidation.None;
            return false;
        }

        #endregion

        #endregion
    }
}
