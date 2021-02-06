using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Ganymed.Console.Processor
{
    internal sealed class GetterInfo : GetterSetterInfo
    {
        /// <summary>
        /// returns the value of the Property/Field
        /// </summary>
        /// <returns></returns>
        internal string GetValue()
        {
            var value = IsProperty
                ? (MemberInfo as PropertyInfo)?.GetValue(null)
                : (MemberInfo as FieldInfo)?.GetValue(null);
                
            if (value is IGettable gettable)
                return gettable.GetterValue();
            
            return value?.ToString() ?? "null";
        }

        /// <summary>
        /// Create a new instance of GetterInfo granting access to metadata of getter attributes
        /// </summary>
        /// <param name="declaringKey"></param>
        /// <param name="memberKey"></param>
        /// <param name="memberInfo"></param>
        /// <param name="memberInfoType"></param>
        /// <param name="isNative"></param>
        /// <param name="priority"></param>
        /// <param name="shortCut"></param>
        /// <param name="description"></param>
        internal GetterInfo(string declaringKey, string memberKey, MemberInfo memberInfo, Type memberInfoType, bool isNative,
            int priority, [CanBeNull] string shortCut, [CanBeNull] string description)
            : base(declaringKey, memberKey, memberInfo, memberInfoType, isNative, priority, shortCut, description) { }
    }
}