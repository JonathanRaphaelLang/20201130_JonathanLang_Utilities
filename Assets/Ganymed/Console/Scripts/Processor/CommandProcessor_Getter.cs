using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ganymed.Console.Attributes;
using Ganymed.Console.Transmissions;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Console.Processor
{
    /// <summary>
    /// partial CommandProcessor class responsible for Getter
    /// </summary>
    public static partial class CommandProcessor
    {
        #region --- [COMMAND] ---

        [NativeCommand]
        [ConsoleCommand(GetterKey, Priority = 10000, Description = GetterDescription)]
        private static void GetCommand(string key)
        {
            if (key == null) LogGetter();
            else if (GetterShortcuts.TryGetValue(key, out var getterShortcut))
            {
                var message =
                    $"{Core.Console.ColorTitleSub.AsRichText()}" +
                    $"[{getterShortcut.MemberKey}]" +
                    $"{RichText.ClearColor}: " +
                    $"{Core.Console.ColorVariables.AsRichText()}" +
                    $"[{getterShortcut.GetValue()}]" +
                    $"{RichText.ClearColor} " +
                    $"Type: {getterShortcut.ValueType.Name} " +
                    $"{(getterShortcut.Description != null ? $"Description: '{getterShortcut.Description}'" : "")}";

                Core.Console.Log(message, LogOptions.Tab);
            }
            else
            {
                key.SimpleTarget(out var args);

                if (args.Length != 2) return;
                if (!Getter.TryGetValue(args[0], out var value)) return;
                if (value.TryGetValue(args[1], out var getter))
                {
                    var message =
                        $"{Core.Console.ColorTitleSub.AsRichText()}" +
                        $"[{getter.MemberKey}]" +
                        $"{RichText.ClearColor}: " +
                        $"{Core.Console.ColorVariables.AsRichText()}" +
                        $"[{getter.GetValue()}]" +
                        $"{RichText.ClearColor} " +
                        $"Type: {getter.ValueType.Name} " +
                        $"{(getter.Description != null ? $"Description: '{getter.Description}'" : "")}";
                    
                    Core.Console.Log(message, LogOptions.Tab);
                }
            }
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [LOG GETTER] ---

        private static void LogGetter(bool separateShortcuts = false)
        {
            if(!Transmission.Start(TransmissionOptions.Enumeration)) return;

            Transmission.AddTitle($"{nameof(CommandProcessor)} Getter");
            Transmission.AddBreak();

            if (separateShortcuts)
            {
                Transmission.AddLine(
                    new MessageFormat($"> Key", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Type", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Shortcut", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Member Type", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Priority",Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Description", Core.Console.ColorTitleSub, MessageOptions.Brackets));
                
                foreach (var member in GetterShortcuts)
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

            foreach (var target in Getter)
            {
                Transmission.AddLine(
                    new MessageFormat($"> Key", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Type", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Shortcut", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Member Type", Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Priority",Core.Console.ColorTitleSub, MessageOptions.Brackets),
                    new MessageFormat("Description", Core.Console.ColorTitleSub, MessageOptions.Brackets));

                Transmission.AddBreak();

                #region --- [LIST & SETUP] ---

                var list = target.Value.Values.ToList();
                list.Sort(delegate(GetterInfo a, GetterInfo b)
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
                        new MessageFormat(list[i].ShortCut ?? string.Empty, Core.Console.ColorEmphasize),
                        // --- Field/Property Type
                        list[i].MemberInfoType.Name.Replace("Info", string.Empty),
                        // --- Priority
                        new MessageFormat(list[i].Priority, MessageOptions.Brackets),
                        // --- Description
                        list[i].Description);

                    if (i >= list.Count - 1 || i <= 0) continue;
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

        private static void FindGetterInAssembly(IEnumerable<Type> types)
        {
            foreach (var type in types)
            {
                #region --- [FIELDS] ---

                foreach (var fieldInfo in type.GetFields(CommandFlags))
                {
                    if (fieldInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute getSetAttribute)
                    {
                        if (getSetAttribute.HideInBuild && !Application.isEditor) continue;
                        if (fieldInfo.DeclaringType is null) continue;
                        
                        var DeclaringKey = CommandProcessor.GetDeclaringKey(fieldInfo.DeclaringType);
                        var memberKey = ValidateGetterMemberKey(DeclaringKey, fieldInfo.Name);
                        var native = fieldInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateGetterShortcut(getSetAttribute.Shortcut);
                        var getterInfo = new GetterInfo(
                            declaringKey: DeclaringKey,
                            memberKey: memberKey,
                            memberInfo: fieldInfo,
                            memberInfoType: typeof(FieldInfo),
                            isNative: native,
                            priority: getSetAttribute.Priority,
                            shortCut: shortCut,
                            description: getSetAttribute.Description);

                        Getter[DeclaringKey].Add(memberKey, getterInfo);

                        TryAddGetterShortCut(shortCut, getterInfo);
                    }
                    else if (fieldInfo.GetCustomAttribute(typeof(GetterAttribute)) is GetterAttribute getterAttribute)
                    {
                        if (getterAttribute.HideInBuild && !Application.isEditor) continue;
                        if (fieldInfo.DeclaringType is null) continue;
                        
                        var DeclaringKey = CommandProcessor.GetDeclaringKey(fieldInfo.DeclaringType);
                        var memberKey = ValidateGetterMemberKey(DeclaringKey, fieldInfo.Name);
                        var native = fieldInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateGetterShortcut(getterAttribute.Shortcut);
                        var getterInfo = new GetterInfo(
                            declaringKey: DeclaringKey,
                            memberKey: memberKey,
                            memberInfo: fieldInfo,
                            memberInfoType: typeof(FieldInfo),
                            isNative: native,
                            priority: getterAttribute.Priority,
                            shortCut: shortCut,
                            description: getterAttribute.Description);

                        Getter[DeclaringKey].Add(memberKey, getterInfo);

                        TryAddGetterShortCut(shortCut, getterInfo);
                    }
                }

                #endregion

                #region --- [PROPERTIES] ---

                foreach (var propertyInfo in type.GetProperties(CommandProcessor.CommandFlags))
                {
                    if (propertyInfo.GetCustomAttribute(typeof(GetSetAttribute)) is GetSetAttribute getSetAttribute)
                    {
                        if (getSetAttribute.HideInBuild && !Application.isEditor) continue;
                        if (propertyInfo.DeclaringType is null || !propertyInfo.CanRead) continue;
                        
                        var declaringKey = CommandProcessor.GetDeclaringKey(propertyInfo.DeclaringType);
                        var memberKey = ValidateGetterMemberKey(declaringKey, propertyInfo.Name);
                        var native = propertyInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateGetterShortcut(getSetAttribute.Shortcut);
                        var getterInfo = new GetterInfo(
                            declaringKey: declaringKey,
                            memberKey: memberKey,
                            memberInfo: propertyInfo,
                            memberInfoType: typeof(PropertyInfo),
                            isNative: native,
                            priority: getSetAttribute.Priority,
                            shortCut: shortCut,
                            description: getSetAttribute.Description);

                        Getter[declaringKey].Add(memberKey, getterInfo);

                        TryAddGetterShortCut(shortCut, getterInfo);
                    }
                    else if (propertyInfo.GetCustomAttribute(typeof(GetterAttribute)) is GetterAttribute getterAttribute)
                    {
                        if (getterAttribute.HideInBuild && !Application.isEditor) continue;
                        if (propertyInfo.DeclaringType is null) continue;
                        
                        var declaringKey = CommandProcessor.GetDeclaringKey(propertyInfo.DeclaringType);
                        var memberKey = ValidateGetterMemberKey(declaringKey, propertyInfo.Name);
                        var native = propertyInfo.GetCustomAttribute(typeof(NativeCommandAttribute)) != null;
                        var shortCut = ValidateGetterShortcut(getterAttribute.Shortcut);

                        var getterInfo = new GetterInfo(
                            declaringKey: declaringKey,
                            memberKey: memberKey,
                            memberInfo: propertyInfo,
                            memberInfoType: typeof(PropertyInfo),
                            isNative: native,
                            priority: getterAttribute.Priority,
                            shortCut: shortCut,
                            description: getterAttribute.Description);

                        Getter[declaringKey].Add(memberKey, getterInfo);
                        TryAddGetterShortCut(shortCut, getterInfo);
                    }
                }

                #endregion
            }
        }

        private static string ValidateGetterShortcut(string shortcut)
        {
            if (shortcut == null) return null;

            if (!GetterShortcuts.ContainsKey(shortcut)) return shortcut;

            while (GetterShortcuts.ContainsKey(shortcut))
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


        private static void TryAddGetterShortCut(string shortcut, GetterInfo getterInfo)
        {
            if (shortcut == null) return;
            GetterShortcuts.Add(shortcut, getterInfo);
        }

        private static string ValidateGetterMemberKey(string declaringKey, string memberKey)
        {
            if (!Getter.ContainsKey(declaringKey)) Getter.Add(declaringKey, new Dictionary<string, GetterInfo>());
            if (!Getter[declaringKey].ContainsKey(memberKey))
            {
                return memberKey;
            }
            else
            {
                while (Getter[declaringKey].ContainsKey(memberKey))
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

        internal static void ProcessGetter(string input)
        {
            PrepareInput(input, out var key);
            GetCommand(key);
        }

        #endregion

        #region --- [PROPOSE] ---

        private static bool ProposeGetter(string input, out string description, out string proposal,
            out InputValidation validation)
        {
            var rawInput = input;
            description = rawInput;
            proposal = rawInput;
            validation = InputValidation.Optional;


            if (!PrepareInput(input, out var key))
            {
                description = $"{rawInput} // receive a list of available getter";

                var shortcutList = new List<GetterInfo>(GetterShortcuts.Values);
                shortcutList.Sort(delegate(GetterInfo a, GetterInfo b)
                {
                    if (a.Priority > b.Priority) return -1;
                    if (b.Priority > a.Priority) return 1;
                    return 0;
                });
                var msg = shortcutList.FirstOrDefault()?.MemberKey ?? Getter.Keys.FirstOrDefault();

                proposal = $"{rawInput} {msg}";
                return true;
            }

            key.SimpleTarget(out var args);

            if (args.Length >= 1)
            {
                if (GetterShortcuts.ContainsKey(args[0]))
                {
                    #region --- [RETURN] ---

                    validation = InputValidation.Valid;
                    return true;

                    #endregion
                }

                var shortcutList = new List<GetterInfo>(GetterShortcuts.Values);
                shortcutList.Sort(delegate(GetterInfo a, GetterInfo b)
                {
                    if (a.Priority > b.Priority) return -1;
                    if (b.Priority > a.Priority) return 1;
                    return 0;
                });

                foreach (var targetPair in shortcutList.Where(targetPair =>
                    targetPair.ShortCut != null && targetPair.ShortCut.StartsWith(args[0])))
                {
                    proposal = $"{rawInput}{targetPair.ShortCut?.Remove(0, args[0].Length)}";
                    description = $"{rawInput}{targetPair.ShortCut?.Remove(0, args[0].Length)}";
                    validation = InputValidation.Incomplete;
                    return true;
                }

                if (Getter.TryGetValue(args[0], out var getterDictionary) && args.Length > 1)
                {
                    if (getterDictionary.ContainsKey(args[1]))
                    {
                        validation = InputValidation.Valid;
                        return true;
                    }

                    var getterList = new List<GetterInfo>(getterDictionary.Values);
                    getterList.Sort(delegate(GetterInfo a, GetterInfo b)
                    {
                        if (a.Priority > b.Priority) return -1;
                        if (b.Priority > a.Priority) return 1;
                        return 0;
                    });

                    foreach (var getterKeys in getterList.Where(getterKeys => getterKeys.MemberKey.StartsWith(args[1])))
                    {
                        proposal = $"{rawInput}{getterKeys.MemberKey.Remove(0, args[1].Length)}";
                        description = $"{rawInput}{getterKeys.MemberKey.Remove(0, args[1].Length)}";
                        validation = InputValidation.Incomplete;
                        return true;
                    }
                }
                else
                {
                    // --- Target / Declaring
                    foreach (var targetPair in Getter.Where(targetPair => targetPair.Key.StartsWith(args[0]))
                        .Where(targetPair => args.Length == 1))
                    {
                        proposal = $"{rawInput}{targetPair.Key.Remove(0, args[0].Length)}.";
                        description = $"{rawInput}{targetPair.Key.Remove(0, args[0].Length)}";
                        validation = InputValidation.Incomplete;
                        return true;
                    }
                }
            }

            validation = InputValidation.Incorrect;
            return false;
        }

        private static bool PrepareInput(string input, out string key)
        {
            key = input.EndsWith(" ") ? string.Empty : null;
            var split = input.Cut().Split(' ');
            if (split.Length > 1) key = split[split.Length - 1].Cut();
            return key != null;
        }

        #endregion
    }
}