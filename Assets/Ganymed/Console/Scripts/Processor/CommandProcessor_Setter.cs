using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Ganymed.Console.Attributes;
using Ganymed.Console.Core;
using Ganymed.Console.Transmissions;
using Ganymed.Utils;
using Ganymed.Utils.ExtensionMethods;
using Ganymed.Utils.Structures;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    
    /// <summary>
    /// partial CommandProcessor class responsible for Setter
    /// </summary>
    public static partial class CommandProcessor
    {
        #region --- [COMMAND] ---

        [NativeCommand]
        [ConsoleCommand(SetterKey, Priority = 10000, Description = SetterDescription)]
        private static void SetterCommand(
            [Hint("Case sensitive input", Show = HintConfig.None)]
            string key,
            [Hint("Value", Show = HintConfig.None)]
            object value)
        {
            if (key == null) LogSetter();
            else if (SetterShortcuts.TryGetValue(key, out var setterShortcut))
            {
                if (TryConvertAndSetValue(setterShortcut.SetValue, value, setterShortcut.ValueType, 
                    ExceptionCallback: delegate
                    {
                        Core.Console.Log(
                            $"Could not convert " +
                            $"{RichText.Red}[{value ?? "null"}]{RichText.ClearColor}" +
                            $" in type " +
                            $"{RichText.Orange}[{setterShortcut.ValueType.Name}]{RichText.ClearColor}", LogOptions.Tab);
                    }))
                {
                    var message =
                        $"Successfully set " +
                        $"{ConsoleSettings.ColorTitleSub.AsRichText()}" +
                        $"[{setterShortcut.MemberKey}]" +
                        $"{RichText.ClearColor} " +
                        $"to " +
                        $"{ConsoleSettings.ColorVariables.AsRichText()}" +
                        $"[{setterShortcut.GetValue() ?? value?.ToString()}]" +
                        $"{RichText.ClearColor} " +
                        $"Type: {setterShortcut.ValueType.Name} " +
                        $"{(setterShortcut.Description != null ? $"Description: '{setterShortcut.Description}'" : "")}";
                    Core.Console.Log(message, LogOptions.Tab);
                }
            }
            else
            {
                key.SimpleTarget(out var args);
                if (args.Length != 2) return;
                if (!Setter.TryGetValue(args[0], out var setterInfos)) return;
                if (setterInfos.TryGetValue(args[1], out var setter))
                {
                    if (TryConvertAndSetValue(setter.SetValue, value, setter.ValueType, 
                        ExceptionCallback: delegate
                    {
                        Core.Console.Log(
                            $"Could not convert " +
                            $"{RichText.Red}[{value ?? "null"}]{RichText.ClearColor}" +
                            $" in type " +
                            $"{RichText.Orange}[{setter.ValueType.Name}]{RichText.ClearColor}", LogOptions.Tab);
                    }))
                    {
                        var message =
                            $"Successfully set " +
                            $"{ConsoleSettings.ColorTitleSub.AsRichText()}" +
                            $"[{setter.MemberKey}]" +
                            $"{RichText.ClearColor} " +
                            $"to " +
                            $"{ConsoleSettings.ColorVariables.AsRichText()}" +
                            $"[{setter.GetValue() ?? value?.ToString()}]" +
                            $"{RichText.ClearColor} " +
                            $"Type: {setter.ValueType.Name} " +
                            $"{(setter.Description != null ? $"Description: '{setter.Description}'" : "")}";
                        Core.Console.Log(message, LogOptions.Tab);
                    }
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONVERTION] ---

        /// <summary>
        /// Method will try to set the value of the given property/field "set value" Method. 
        /// </summary>
        /// <param name="Set">The Property/Field "Set Value" Method</param>
        /// <param name="value">The value</param>
        /// <param name="type"></param>
        /// <param name="ExceptionCallback"></param>
        /// <returns></returns>
        private static bool TryConvertAndSetValue(Action<object> Set, object value, Type type, Action ExceptionCallback)
        {
            try
            {
                #region --- [BOOLEAN] ---

                if (type == typeof(bool) && int.TryParse(value.ToString(), out var integer))
                {
                    Set?.Invoke(integer > 0);
                    return true;
                }

                if (type == typeof(bool))
                {
                    if(string.Equals(value.ToString().Cut(), true.ToString(), StringComparison.OrdinalIgnoreCase))
                        Set?.Invoke(true);
                    if(string.Equals(value.ToString().Cut(), false.ToString(), StringComparison.OrdinalIgnoreCase))
                        Set?.Invoke(false);
                }

                #endregion

                #region --- [CHAR] ---

                else if (type == typeof(char))
                {
                    Set?.Invoke(float.Parse(Regex.Replace(value.ToString().Cut().Replace('.', ','), "[^., 0-9]", "")));
                    return true;
                }

                #endregion

                #region --- [PRIMITIVE] ---

                else if (type.IsPrimitive)
                {
                    Set?.Invoke(
                        Convert.ChangeType(Regex.Replace(value.ToString().Cut().Replace('.', ','), "[^., 0-9]", ""),
                            type));
                    return true;
                }

                #endregion

                #region --- [ENUM] ---

                else if (type.IsEnum)
                {
                    var valueString = value.ToString().Cut();
                    var enumValues = Enum.GetValues(type);

                    var SetEnum = 0;

                    var args = valueString.Split('|');

                    foreach (var argument in args)
                    {
                        if (int.TryParse(argument.Cut(), out var num))
                        {
                            foreach (var enumValue in enumValues)
                            {
                                if (num == (int) enumValue)
                                {
                                    SetEnum |= num;
                                }
                            }
                        }
                        else
                        {
                            foreach (var enumValue in enumValues)
                            {
                                if (string.Equals(enumValue.ToString(), argument.Cut(),
                                    StringComparison.CurrentCultureIgnoreCase))
                                {
                                    SetEnum |= (int) enumValue;
                                }
                            }
                        }
                    }

                    Set?.Invoke(SetEnum);
                    return true;
                }

                #endregion

                #region --- [VECTOR/COLOR] ---

                else if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
                         type == typeof(Color) || type == typeof(Color32) ||
                         type == typeof(Vector2Int) || type == typeof(Vector3Int) || type == typeof(Vector4Int))
                {
                    var args = value.ToString().AsStructInputArgs();
                    switch (args.Length)
                    {
                        case 2 when type == typeof(Vector2):
                            value = new Vector2(
                                x: float.Parse(args[0]),
                                y: float.Parse(args[1]));
                            Set?.Invoke(value);
                            return true;

                        case 2 when type == typeof(Vector2Int):
                            value = new Vector2Int(
                                x: int.Parse(args[0]),
                                y: int.Parse(args[1]));
                            Set?.Invoke(value);
                            return true;


                        case 3 when type == typeof(Vector3):
                            value = new Vector3(
                                x: float.Parse(args[0]),
                                y: float.Parse(args[1]),
                                z: float.Parse(args[2]));
                            Set?.Invoke(value);
                            return true;

                        case 3 when type == typeof(Vector3Int):
                            value = new Vector3Int(
                                x: int.Parse(args[0]),
                                y: int.Parse(args[1]),
                                z: int.Parse(args[2]));
                            Set?.Invoke(value);
                            return true;


                        case 4 when type == typeof(Vector4):
                            value = new Vector4(
                                x: float.Parse(args[0]),
                                y: float.Parse(args[1]),
                                z: float.Parse(args[2]),
                                w: float.Parse(args[3]));
                            Set?.Invoke(value);
                            return true;


                        case 4 when type == typeof(Vector4Int):
                            value = new Vector4(
                                x: int.Parse(args[0]),
                                y: int.Parse(args[1]),
                                z: int.Parse(args[2]),
                                w: int.Parse(args[3]));
                            Set?.Invoke(value);
                            return true;


                        case 4 when type == typeof(Color):
                            value = new Color(
                                r: Mathf.Clamp(float.Parse(args[0]), 0, 1),
                                g: Mathf.Clamp(float.Parse(args[1]), 0, 1),
                                b: Mathf.Clamp(float.Parse(args[2]), 0, 1),
                                a: Mathf.Clamp(float.Parse(args[3]), 0, 1));
                            Set?.Invoke(value);
                            return true;

                        case 4 when type == typeof(Color32):
                            value = new Color32(
                                r: Convert.ToByte(Mathf.Clamp(int.Parse(args[0]), 0, 255)),
                                g: Convert.ToByte(Mathf.Clamp(int.Parse(args[1]), 0, 255)),
                                b: Convert.ToByte(Mathf.Clamp(int.Parse(args[2]), 0, 255)),
                                a: Convert.ToByte(Mathf.Clamp(int.Parse(args[3]), 0, 255)));
                            Set?.Invoke(value);
                            return true;
                    }
                }

                #endregion

                #region --- [STRING] ---

                else if (type == typeof(string))
                {
                    Set?.Invoke(value.ToString().Cut());
                    return true;
                }

                #endregion

                else
                {
                    Set?.Invoke(Convert.ChangeType(value.ToString().Cut(), type));
                    return true;
                }
            }
            catch
            {
                ExceptionCallback?.Invoke();
                return false;
            }
            return true;
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [LOG SETTER] ---

        private static void LogSetter(bool separateShortcuts = false)
        {
            if(!Transmission.Start(TransmissionOptions.Enumeration)) return;

            Transmission.AddTitle($"{nameof(CommandProcessor)} Setter");
            Transmission.AddBreak();

            if (separateShortcuts)
            {
                Transmission.AddLine(
                    new MessageFormat($"> Key", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Type", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Shortcut", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Member Type", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Priority",ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Description", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets));

                foreach (var member in SetterShortcuts)
                {
                    var type = member.Value.ValueType;
                    
                    Transmission.AddLine(
                        $"{member.Key}",
                        $"{(type.IsEnum? $"{type.Name} (Enum)" : type.Name)}",
                        member.Value.MemberInfoType.Name.Replace("Info", string.Empty),
                        member.Value.ShortCut,
                        member.Value.Description,
                        $"{member.Value.Priority:00}");
                }

                Transmission.AddBreak();
            }

            foreach (var target in Setter)
            {
                Transmission.AddLine(
                    new MessageFormat($"> Key", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Type", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Shortcut", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Member Type", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Priority",ConsoleSettings.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Description", ConsoleSettings.ColorTitleSub, MessageOptions.Brackets));

                Transmission.AddBreak();

                #region --- [LIST & SETUP] ---

                var list = target.Value.Values.ToList();
                list.Sort(delegate(SetterInfo a, SetterInfo b)
                {
                    if (a.IsField && !b.IsField)  return 1; 
                    if (b.IsField && !a.IsField)  return -1; 
                    if (a.Priority > b.Priority)  return -1; 
                    if (b.Priority > a.Priority)  return 1; 
                    return 0;
                });

                #endregion

                for (var i = 0; i < list.Count; i++)
                {
                    var type = list[i].ValueType;
                    Transmission.AddLine(
                        // --- Key
                        $"{target.Key}.{list[i].MemberKey}",
                        // --- ValueType
                        $"{(type.IsEnum? $"{type.Name} (Enum)" : type.Name)}",
                        // --- Shortcut
                        new MessageFormat(list[i].ShortCut ?? string.Empty, ConsoleSettings.ColorEmphasize),
                        // --- Field/Property Type
                        list[i].MemberInfoType.Name.Replace("Info", string.Empty),
                        // --- Priority
                        new MessageFormat(list[i].Priority, MessageOptions.Brackets),
                        // --- Description
                        list[i].Description);

                    if (i < list.Count - 1 && i > 0)
                        if (list[i].IsProperty && list[i + 1].IsField)
                            Transmission.AddBreak();
                }

                Transmission.AddBreak();
            }

            Transmission.ReleaseAsync();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [INITIALIZATION] ---

        private static void FindSetterInAssembly(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                #region --- [FIELDS] ---

                foreach (var fieldInfo in type.GetFields(CommandProcessor.CommandFlags))
                {
                    if (fieldInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute getSetAttribute)
                    {
                        if (getSetAttribute.HideInBuild && !Application.isEditor) continue;
                        if (fieldInfo.DeclaringType is null) continue;

                        var declaringKey = CommandProcessor.GetDeclaringKey(fieldInfo.DeclaringType);
                        var memberKey = ValidateSetterMemberKey(declaringKey, fieldInfo.Name);
                        var native = fieldInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateSetterShortcut(getSetAttribute.Shortcut);
                        var setterInfo = new SetterInfo(
                            declaringKey: declaringKey,
                            memberKey: memberKey,
                            memberInfo: fieldInfo,
                            memberInfoType: typeof(FieldInfo),
                            isNative: native,
                            priority: getSetAttribute.Priority,
                            shortCut: shortCut,
                            description: getSetAttribute.Description,
                            defaultValue: getSetAttribute.Default);

                        Setter[declaringKey].Add(memberKey, setterInfo);

                        TryAddSetterShortCut(shortCut, setterInfo);
                    }

                    else if (fieldInfo.GetCustomAttribute(typeof(SetterAttribute)) is SetterAttribute setterAttribute)
                    {
                        if (setterAttribute.HideInBuild && !Application.isEditor) continue;
                        if (fieldInfo.DeclaringType is null) continue;
                        
                        var declaringKey = CommandProcessor.GetDeclaringKey(fieldInfo.DeclaringType);
                        var memberKey = ValidateSetterMemberKey(declaringKey, fieldInfo.Name);
                        var native = fieldInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateSetterShortcut(setterAttribute.Shortcut);
                        var setterInfo = new SetterInfo(
                            declaringKey: declaringKey,
                            memberKey: memberKey,
                            memberInfo: fieldInfo,
                            memberInfoType: typeof(FieldInfo),
                            isNative: native,
                            priority: setterAttribute.Priority,
                            shortCut: shortCut,
                            description: setterAttribute.Description,
                            defaultValue: setterAttribute.Default);

                        Setter[declaringKey].Add(memberKey, setterInfo);

                        TryAddSetterShortCut(shortCut, setterInfo);
                    }
                }

                #endregion

                #region --- [PROPERTIES] ---

                foreach (var propertyInfo in type.GetProperties(CommandProcessor.CommandFlags))
                {
                    if (propertyInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute getSetAttribute)
                    {
                        if (getSetAttribute.HideInBuild && !Application.isEditor) continue;
                        
                        if (propertyInfo.DeclaringType is null || !propertyInfo.CanWrite) continue;
                        var declaringKey = CommandProcessor.GetDeclaringKey(propertyInfo.DeclaringType);
                        var memberKey = ValidateSetterMemberKey(declaringKey, propertyInfo.Name);
                        var native = propertyInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateSetterShortcut(getSetAttribute.Shortcut);

                        var setterInfo = new SetterInfo(
                            declaringKey: declaringKey,
                            memberKey: memberKey,
                            memberInfo: propertyInfo,
                            memberInfoType: typeof(PropertyInfo),
                            isNative: native,
                            priority: getSetAttribute.Priority,
                            shortCut,
                            description: getSetAttribute.Description,
                            defaultValue: getSetAttribute.Default);

                        Setter[declaringKey].Add(memberKey, setterInfo);

                        TryAddSetterShortCut(shortCut, setterInfo);
                    }
                    else if (propertyInfo.GetCustomAttribute(typeof(SetterAttribute)) is SetterAttribute setterAttribute)
                    {
                        if (setterAttribute.HideInBuild && !Application.isEditor) continue;
                        if (propertyInfo.DeclaringType is null) continue;
                        
                        var declaringKey = CommandProcessor.GetDeclaringKey(propertyInfo.DeclaringType);
                        var memberKey = ValidateSetterMemberKey(declaringKey, propertyInfo.Name);
                        var native = propertyInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateSetterShortcut(setterAttribute.Shortcut);

                        var setterInfo = new SetterInfo(
                            declaringKey: declaringKey,
                            memberKey: memberKey,
                            memberInfo: propertyInfo,
                            memberInfoType: typeof(PropertyInfo),
                            isNative: native,
                            priority: setterAttribute.Priority,
                            shortCut: shortCut,
                            description: setterAttribute.Description,
                            defaultValue: setterAttribute.Default);

                        Setter[declaringKey].Add(memberKey, setterInfo);

                        TryAddSetterShortCut(shortCut, setterInfo);
                    }
                }

                #endregion
            }
        }

        private static string ValidateSetterShortcut(string shortcut)
        {
            if (shortcut == null) return null;

            if (!SetterShortcuts.ContainsKey(shortcut)) return shortcut;

            while (SetterShortcuts.ContainsKey(shortcut))
            {
                if (char.IsDigit(shortcut[shortcut.Length - 1]))
                {
                    var removedCharacters = new List<char>();
                    while (char.IsDigit(shortcut[shortcut.Length - 1]))
                    {
                        removedCharacters.Add(shortcut[shortcut.Length - 1]);
                        shortcut = shortcut.Remove(shortcut.Length - 1, 1);
                    }

                    removedCharacters.Reverse();
                    var idString =
                        removedCharacters.Aggregate(string.Empty, (current, character) => current + character);
                    var id = int.Parse(idString);
                    id++;
                    shortcut = $"{shortcut}{id}";
                }
                else
                {
                    shortcut = $"{shortcut}{1}";
                }
            }

            return shortcut;
        }


        private static void TryAddSetterShortCut(string shortcut, SetterInfo setterInfo)
        {
            if (shortcut == null) return;
            SetterShortcuts.Add(shortcut, setterInfo);
        }

        private static string ValidateSetterMemberKey(string declaringKey, string memberKey)
        {
            if (!Setter.ContainsKey(declaringKey)) Setter.Add(declaringKey, new Dictionary<string, SetterInfo>());
            if (!Setter[declaringKey].ContainsKey(memberKey))
            {
                return memberKey;
            }
            else
            {
                while (Setter[declaringKey].ContainsKey(memberKey))
                {
                    if (char.IsDigit(memberKey[memberKey.Length - 1]))
                    {
                        var removedCharacters = new List<char>();
                        while (char.IsDigit(memberKey[memberKey.Length - 1]))
                        {
                            removedCharacters.Add(memberKey[memberKey.Length - 1]);
                            memberKey = memberKey.Remove(memberKey.Length - 1, 1);
                        }

                        removedCharacters.Reverse();
                        var idString =
                            removedCharacters.Aggregate(string.Empty, (current, character) => current + character);
                        var id = int.Parse(idString);
                        id++;
                        memberKey = $"{memberKey}{id}";
                    }
                    else
                    {
                        memberKey = $"{memberKey}{1}";
                    }
                }

                return memberKey;
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [PROCESS] ---

        internal static void ProcessSetter(string input)
        {
            var args = input.Cut().Remove(0, CommandProcessor.Prefix.Length).Cut()
                .Remove(0, CommandProcessor.SetterKey.Length).Cut().Split(' ');

            var key = string.IsNullOrWhiteSpace(args[0]) ? null : args[0];
            var obj = args.Length > 1 ? string.Empty : null;

            for (var i = 1; i < args.Length; i++)
            {
                obj += $" {args[i]}";
            }

            SetterCommand(key, obj);
        }

        #endregion

        #region --- [PROPOSE] ---

        private static bool ProposeSetter(string input, out string description, out string proposal,
            out InputValidation validation)
        {
            var rawInput = input;
            
            description = rawInput;
            proposal = rawInput;
            validation = InputValidation.Optional;


            if (!PrepareInput(input, out var key, out var value))
            {
                var shortcutList = new List<SetterInfo>(SetterShortcuts.Values);
                shortcutList.Sort(delegate(SetterInfo a, SetterInfo b)
                {
                    if (a.Priority > b.Priority) return -1;
                    return b.Priority > a.Priority ? 1 : 0;
                });
                var msg = shortcutList.FirstOrDefault()?.MemberKey ?? Getter.Keys.FirstOrDefault();

                if (!input.EndsWith(" "))
                {
                    description = $"{rawInput} // receive a list of available setter";
                    proposal = $"{rawInput} {msg}";
                    return true;
                }

                description = $"{rawInput}{msg}";
                proposal = description;
                return true;
            }

            rawInput.RemoveSetterPrefix(out var target);

            var target2 = target.Length > 1 ? target[1] : string.Empty;

            if (target.Length >= 1)
            {
                if (SetterShortcuts.TryGetValue(target[0], out var setterInfo))
                {
                    #region --- [RETURN] ---

                    if (value?.Length > 0)
                    {
                        if (TryConvert(value, setterInfo.ValueType) && !setterInfo.ValueType.IsEnum)
                        {
                            validation = InputValidation.Valid;
                            return true;
                        }

                        if (ProposeValue(value, setterInfo.ValueType, out var proposedValue, out var optional))
                        {
                            validation = optional ? InputValidation.Optional : InputValidation.Incomplete;
                            description = $"{rawInput}{proposedValue}";
                            proposal = description;
                            return true;
                        }


                        validation = InputValidation.Incorrect;
                        proposal = $"{rawInput}{setterInfo.GetDefaultValue()}";
                        return true;
                    }

                    if (rawInput.EndsWith(" "))
                    {
                        description = $"{rawInput} // {setterInfo.ValueType.Name}";
                        proposal = $"{rawInput}{setterInfo.GetDefaultValue()}";
                        return true;
                    }

                    validation = InputValidation.Incomplete;
                    proposal = $"{rawInput} {setterInfo.GetDefaultValue()}";
                    return true;

                    #endregion
                }

                var setterList = new List<SetterInfo>(SetterShortcuts.Values);
                setterList.Sort(delegate(SetterInfo a, SetterInfo b)
                {
                    if (a.Priority > b.Priority) return -1;
                    if (b.Priority > a.Priority) return 1;
                    return 0;
                });

                foreach (var targetPair in setterList.Where(targetPair =>
                    targetPair.ShortCut != null && targetPair.ShortCut.StartsWith(target[0])))
                {
                    proposal = $"{rawInput}{targetPair.ShortCut?.Remove(0, target[0].Length)}";
                    description = proposal;
                    validation = InputValidation.Incomplete;
                    return true;
                }

                if (Setter.TryGetValue(target[0], out var setterInfos) && target.Length > 1)
                {
                    if (setterInfos.TryGetValue(target[1], out setterInfo))
                    {
                        #region --- [RETURN] ---

                        if (value?.Length > 0)
                        {
                            if (TryConvert(value, setterInfo.ValueType) && !setterInfo.ValueType.IsEnum)
                            {
                                validation = InputValidation.Valid;
                                proposal = rawInput;
                                return true;
                            }

                            if (ProposeValue(value, setterInfo.ValueType, out var proposedValue, out var optional))
                            {
                                validation = optional ? InputValidation.Optional : InputValidation.Incomplete;
                                description = $"{rawInput}{proposedValue}";
                                proposal = description;
                                return true;
                            }


                            validation = InputValidation.Incorrect;
                            proposal = $"{rawInput}{setterInfo.GetDefaultValue()}";
                            return true;
                        }

                        if (rawInput.EndsWith(" "))
                        {
                            description = $"{rawInput} // {setterInfo.ValueType.Name}";
                            proposal = $"{rawInput}{setterInfo.GetDefaultValue()}";
                            return true;
                        }

                        validation = InputValidation.Incomplete;
                        proposal = $"{rawInput} {setterInfo.GetDefaultValue()}";
                        return true;

                        #endregion
                    }

                    var setterKeyList = new List<SetterInfo>(setterInfos.Values);
                    setterKeyList.Sort(delegate(SetterInfo a, SetterInfo b)
                    {
                        if (a.Priority > b.Priority) return -1;
                        if (b.Priority > a.Priority) return 1;
                        return 0;
                    });

                    foreach (var setterKeys in setterKeyList.Where(setterKeys =>
                        setterKeys.MemberKey.StartsWith(target[1])))
                    {
                        proposal = $"{rawInput}{setterKeys.MemberKey.Remove(0, target[1].Length)}";
                        description = proposal;
                        validation = InputValidation.Incomplete;
                        return true;
                    }
                }
                else
                {
                    foreach (var targetPair in Setter.Where(targetPair => targetPair.Key.StartsWith(target[0]))
                        .Where(targetPair => target.Length == 1))
                    {
                        proposal = $"{rawInput}{targetPair.Key.Remove(0, target[0].Length)}.";
                        description = $"{rawInput}{targetPair.Key.Remove(0, target[0].Length)}";
                        validation = InputValidation.Incomplete;
                        return true;
                    }
                }
            }

            validation = InputValidation.Incorrect;
            return false;
        }

        private static bool TryConvert(string input, Type type)
        {
            try
            {
                #region --- [BOOLEAN] ---

                if (type == typeof(bool) && int.TryParse(input.Cut(), out var intBool))
                {
                    return true;
                }
                
                if (type == typeof(bool))
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Convert.ChangeType(input.Cut(), type);
                    return true;
                }

                #endregion

                #region --- [CHAR] ---

                if (type == typeof(char))
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Convert.ChangeType(input.Cut(), type);
                    return true;
                }

                #endregion

                #region --- [PRIMITIVE] ---

                if (type.IsPrimitive)
                {
                    input = Regex.Replace(input.Cut().Replace('.', ','), "[^., 0-9]", "");
                    input = input.StartsWith(",") ? $"0{input}" : input;
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Convert.ChangeType(input, type);
                    return true;
                }

                #endregion

                #region --- [ENUM] ---

                if (type.IsEnum)
                {
                    var enumInput = input;
                    var enumValues = Enum.GetValues(type);

                    var isValid = false;

                    var args = enumInput.Split('|');

                    foreach (var argument in args)
                    {
                        if (int.TryParse(argument.Cut(), out var enumNum))
                        {
                            foreach (var enumValue in enumValues)
                            {
                                if (enumNum == (int) enumValue)
                                {
                                    isValid = true;
                                }
                            }
                        }
                        else
                        {
                            foreach (var enumValue in enumValues)
                            {
                                if (string.Equals(enumValue.ToString(), argument.Cut(),
                                    StringComparison.CurrentCultureIgnoreCase))
                                {
                                    isValid = true;
                                }
                            }
                        }
                    }

                    return isValid;
                }

                #endregion

                #region --- [VECTOR & COLOR] ---

                if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
                    type == typeof(Color) || type == typeof(Color32))
                {
                    var args = input.AsStructInputArgs();

                    switch (args.Length)
                    {
                        case 2 when type == typeof(Vector2):
                            var vector2 = new Vector2(
                                x: float.Parse(args[0]),
                                y: float.Parse(args[1]));
                            return true;

                        case 2 when type == typeof(Vector2Int):
                            var vector2int = new Vector2Int(
                                x: int.Parse(args[0]),
                                y: int.Parse(args[1]));
                            return true;

                        case 3 when type == typeof(Vector3):
                            var vector3 = new Vector3(
                                x: float.Parse(args[0]),
                                y: float.Parse(args[1]),
                                z: float.Parse(args[2]));
                            return true;

                        case 3 when type == typeof(Vector3Int):
                            var vector3int = new Vector3Int(
                                x: int.Parse(args[0]),
                                y: int.Parse(args[1]),
                                z: int.Parse(args[2]));
                            return true;

                        case 4 when type == typeof(Vector4):
                            var vector4 = new Vector4(
                                x: float.Parse(args[0]),
                                y: float.Parse(args[1]),
                                z: float.Parse(args[2]),
                                w: float.Parse(args[3]));
                            return true;

                        case 4 when type == typeof(Vector4Int):
                            var vector4int = new Vector4Int(
                                x: int.Parse(args[0]),
                                y: int.Parse(args[1]),
                                z: int.Parse(args[2]),
                                w: int.Parse(args[3]));
                            return true;

                        case 4 when type == typeof(Color):
                            var color = new Color(
                                r: Mathf.Clamp(float.Parse(args[0]), 0, 1),
                                g: Mathf.Clamp(float.Parse(args[1]), 0, 1),
                                b: Mathf.Clamp(float.Parse(args[2]), 0, 1),
                                a: Mathf.Clamp(float.Parse(args[3]), 0, 1));
                            return true;


                        case 4 when type == typeof(Color32):
                            var color32 = new Color32(
                                r: Convert.ToByte(Mathf.Clamp(int.Parse(args[0]), 0, 255)),
                                g: Convert.ToByte(Mathf.Clamp(int.Parse(args[1]), 0, 255)),
                                b: Convert.ToByte(Mathf.Clamp(int.Parse(args[2]), 0, 255)),
                                a: Convert.ToByte(Mathf.Clamp(int.Parse(args[3]), 0, 255)));
                            return true;
                    }
                }

                #endregion

                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Convert.ChangeType(input, type);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool ProposeValue(string input, Type type, out string proposedValue, out bool optional)
        {
            optional = false;
            proposedValue = string.Empty;

            #region --- [BOOLEAN] ---

            if (type == typeof(bool))
            {
                if (input.EndsWith(" ")) return false;
                if ("true".StartsWith(input, StringComparison.CurrentCultureIgnoreCase))
                {
                    proposedValue = "true".Remove(0, input.Length);
                    return true;
                }

                if ("false".StartsWith(input, StringComparison.CurrentCultureIgnoreCase))
                {
                    proposedValue = "false".Remove(0, input.Length);
                    return true;
                }
            }

            #endregion

            #region --- [ENUM] ---

            if (type.IsEnum)
            {
                var enumValues = Enum.GetValues(type);
                var enumInputs = input.Split('|');
                var focus = enumInputs[enumInputs.Length - 1];
                var focusCut = focus.Cut();

                var list = new List<int>();

                foreach (var enumValue in enumValues)
                {
                    foreach (var enumInput in enumInputs)
                    {
                        if (string.Equals(enumInput.Cut(), enumValue.ToString(),
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            list.Add((int) enumValue);
                        }
                    }
                }


                foreach (var enumValue in enumValues)
                {
                    var enumInt = (int) enumValue;
                    if (string.Equals(focusCut, enumValue.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        optional = true;
                        return true;
                    }

                    if (focusCut == enumInt.ToString())
                    {
                        optional = true;
                        return true;
                    }

                    if (enumValue.ToString().StartsWith(focusCut, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (!list.Contains((int) enumValue))
                        {
                            proposedValue =
                                $"{(focus == string.Empty ? " " : "")}{enumValue.ToString().Remove(0, (focus == string.Empty ? focus.Length : focusCut.Length))}";
                            optional = false;
                            return true;
                        }
                    }

                    if (enumInt.ToString().StartsWith(focusCut))
                    {
                        if (!list.Contains((int) enumInt))
                        {
                            proposedValue =
                                $"{(focus == string.Empty ? " " : "")}{enumInt.ToString().Remove(0, (focus == string.Empty ? focus.Length : focusCut.Length))}";
                            optional = false;
                            return true;
                        }
                    }
                }

                optional = true;
                return true;
            }

            #endregion

            #region --- [COLOR & VECTOR] ---

            if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4) ||
                type == typeof(Color) || type == typeof(Color32) ||
                type == typeof(Vector2Int) || type == typeof(Vector3Int) || type == typeof(Vector4Int))
            {
                var args = input.AsStructInputArgs();
                var maxLenght = type == typeof(Vector2) || type == typeof(Vector2Int)
                    ? 2
                    : type == typeof(Vector3) || type == typeof(Vector3Int)
                        ? 3
                        : 4;


                if (args.Length > maxLenght) return false;
                foreach (var argument in args)
                {
                    try
                    {
                        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                        Convert.ChangeType(argument.Cut(),
                            type == typeof(Color32) ? typeof(byte) : type.IsVector() ? typeof(float) : typeof(int));
                    }
                    catch
                    {
                        return false;
                    }
                }

                proposedValue =
                    $"{(input.EndsWith(" ") ? string.Empty : " ")}{(type == typeof(Color32) ? "255" : "1")}";
                return true;
            }

            #endregion

            return false;
        }

        private static bool PrepareInput(string input, out string key, out string value)
        {
            key = input.EndsWith(" ") ? string.Empty : null;

            var split = input.Cut().Split(' ');

            key = split.Length == 2 ? split[split.Length - 1].Cut() :
                split.Length > 2 ? split[split.Length - 2].Cut() : null;


            var split2 = input.Cut(StartEnd.Start).Split(' ');
            value = split2.Length > 2 ? string.Empty : null;
            if (split2.Length > 2)
            {
                for (var i = 2; i < split2.Length; i++)
                {
                    value += $"{split2[i]} ";
                }

                value = value?.Remove(value.Length - 1, 1);
            }

            return key != null;
        }

        #endregion
    }
}