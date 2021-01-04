using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    public static class GetterProcessor
    {
        #region --- [FIELDS] ---
        
        private static readonly Dictionary<string, FieldInfo> Fields = new Dictionary<string, FieldInfo>();
        private static readonly Dictionary<string, string> FieldsCut = new Dictionary<string, string>();
        
        private static readonly Dictionary<string, PropertyInfo> Properties = new Dictionary<string, PropertyInfo>();
        private static readonly Dictionary<string, string> PropertiesCut = new Dictionary<string, string>();

        private static string CompiledPrefix => $"{CommandProcessor.Prefix}{CommandProcessor.GetKey}";
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMAND] ---

        [NativeCommand]
        [Command(CommandProcessor.GetKey, priority: 10000)]
        private static void GetCommand([CanBeNull] string target)
        {
            if (target == null) GetGetter();
            
            else if (Fields.TryGetValue(target, out var fieldInfo))
            {
                Core.Console.Log(fieldInfo.GetValue(null));
            }
            else if (Properties.TryGetValue(target, out var propertyInfo))
            {
                Core.Console.Log(propertyInfo.GetValue(null));
            }

            else if(FieldsCut.TryGetValue(target, out var i))
            {
                Core.Console.Log(Fields[i].GetValue(null));
            }
            else if(PropertiesCut.TryGetValue(target, out var j))
            {
                Core.Console.Log(Properties[j].GetValue(null));
            }
        }

        //TODO: add target dictionaries
        //TODO: formatting
        
        private static void GetGetter()
        {
            
            Transmission.Start(TransmissionOptions.Enumeration, $"{nameof(CommandProcessor)} Getter");
            
            Transmission.AddTitle("Fields (Get)");
            Transmission.AddLine("Field", "Type", "Abbreviation");
            Transmission.AddBreak();
            
            foreach (var getter in Fields)
            {
                if(FieldsCut.ContainsValue(getter.Key))
                {
                    Transmission.AddLine(getter.Key, getter.Value.FieldType, FieldsCut.FirstOrDefault(x => x.Value == getter.Key).Key);
                }
                else
                {
                    Transmission.AddLine(getter.Key, getter.Value.FieldType);    
                }
            }
            
            Transmission.AddBreak();
            Transmission.AddTitle("Properties (Get)");
            Transmission.AddLine("Property", "Type", "Abbreviation");
            Transmission.AddBreak();
            
            foreach (var getter in Properties)
            {
                if(PropertiesCut.ContainsValue(getter.Key))
                {
                    Transmission.AddLine(getter.Key, getter.Value.PropertyType, PropertiesCut.FirstOrDefault(x => x.Value == getter.Key).Key);
                }
                else
                {
                    Transmission.AddLine(getter.Key, getter.Value.PropertyType);
                }
            }
            Transmission.ReleaseAsync();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        internal static void FindGetterInAssembly()
        {
            var types =  AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(object));

            foreach (var type in types)
            {
               foreach (var fieldInfo in type.GetFields(CommandProcessor.GetterFieldFlags))
               {
                   var native = fieldInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                   
                   if ((fieldInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute attribute))
                   {
                       if (fieldInfo.DeclaringType is null) continue;
                       var key = native? $"{fieldInfo.Name}" : $"{fieldInfo.DeclaringType.Name}.{fieldInfo.Name}";
                       Fields.Add(key, fieldInfo);
                       if(attribute.Shortcut != null) FieldsCut.Add(attribute.Shortcut, key);
                   }
                   else if ((fieldInfo.GetCustomAttribute(typeof(GetterAttribute)) is GetterAttribute setterAttribute))
                   {
                       if (fieldInfo.DeclaringType is null) continue;
                       var key = native? $"{fieldInfo.Name}" : $"{fieldInfo.DeclaringType.Name}.{fieldInfo.Name}";
                       Fields.Add(key, fieldInfo);
                       if(setterAttribute.Shortcut != null) FieldsCut.Add(setterAttribute.Shortcut, key);
                   }
               }
               foreach (var propertyInfo in type.GetProperties(CommandProcessor.GetterPropertyFlags))
               {
                   var native = propertyInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                   
                   if ((propertyInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute attribute))
                   {
                       if (propertyInfo.DeclaringType is null) continue;
                       var key = native? $"{propertyInfo.Name}" : $"{propertyInfo.DeclaringType.Name}.{propertyInfo.Name}";
                       Properties.Add(key, propertyInfo);
                       if(attribute.Shortcut != null) PropertiesCut.Add(attribute.Shortcut, key);
                   }
                   else if ((propertyInfo.GetCustomAttribute(typeof(GetterAttribute)) is GetterAttribute setterAttribute))
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

        internal static bool Propose(string input, out string proposedDescription, out string proposedInput, out InputValidation inputValidation)
        {
            proposedDescription = string.Empty;
            proposedInput = string.Empty;
            inputValidation = InputValidation.None;
            var rawInput = input;

            if (!PrepareInput(input, out var key)) return false;

            #region --- [SEARCH FOR MATCH] ---

            foreach (var pair in FieldsCut.Where(pair => pair.Key.StartsWith(key)))
            {
                if (key.Equals(pair.Key))
                {
                    inputValidation = InputValidation.Valid;
                    return true;
                }

                var proposed = pair.Key.Remove(0, key.Length).Split('.');
                
                proposedDescription = $"{rawInput}{proposed[0]}";
                proposedInput = $"{rawInput}{proposed[0]}{(proposed.Length > 1? "." : string.Empty)}";
                inputValidation = InputValidation.Incomplete;
                
                return true;
            }
            foreach (var pair in PropertiesCut.Where(pair => pair.Key.StartsWith(key)))
            {
                if (key.Equals(pair.Key))
                {
                    inputValidation = InputValidation.Valid;
                    return true;
                }

                var proposed = pair.Key.Remove(0, key.Length).Split('.');
                
                proposedDescription = $"{rawInput}{proposed[0]}";
                proposedInput = $"{rawInput}{proposed[0]}{(proposed.Length > 1? "." : string.Empty)}";
                inputValidation = InputValidation.Incomplete;
                
                return true;
            }
            foreach (var pair in Fields.Where(pair => pair.Key.StartsWith(key)))
            {
                if (key.Equals(pair.Key))
                {
                    inputValidation = InputValidation.Valid;
                    return true;
                }

                var proposed = pair.Key.Remove(0, key.Length).Split('.');
                
                proposedDescription = $"{rawInput}{proposed[0]}";
                proposedInput = $"{rawInput}{proposed[0]}{(proposed.Length > 1? "." : string.Empty)}";
                inputValidation = InputValidation.Incomplete;
                
                return true;
            }
            foreach (var pair in Properties.Where(pair => pair.Key.StartsWith(key)))
            {
                if (key.Equals(pair.Key))
                {
                    inputValidation = InputValidation.Valid;
                    return true;
                }

                var proposed = pair.Key.Remove(0, key.Length).Split('.');
                
                proposedDescription = $"{rawInput}{proposed[0]}";
                proposedInput = $"{rawInput}{proposed[0]}{(proposed.Length > 1? "." : string.Empty)}";
                inputValidation = InputValidation.Incomplete;
                
                return true;
            }
            
            #endregion
            
            return false;
        }

        private static bool PrepareInput(string input, out string key)
        {
            key = input.EndsWith(" ") ? string.Empty : null;
            var split = input.Cut().Split(' ');
            if(split.Length > 1) key = split[1].Cut();
            return key != null;
        }

        #endregion
    }
}
