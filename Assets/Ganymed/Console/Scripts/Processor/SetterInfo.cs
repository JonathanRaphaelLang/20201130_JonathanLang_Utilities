using System;
using System.Reflection;
using Ganymed.Utils.ExtensionMethods;
using JetBrains.Annotations;

namespace Ganymed.Console.Processor
{
    internal sealed class SetterInfo : GetterSetterInfo
    {
        [CanBeNull] private readonly object DefaultValue = null;
        internal object GetDefaultValue() => DefaultValue ?? ValueType.TryGetDefaultInstance();
        
        /// <summary>
        /// Set the value of the property/field
        /// </summary>
        /// <param name="value"></param>
        internal void SetValue(object value)
        {
            switch (MemberInfo)
            {
                case FieldInfo fieldInfo:
                    fieldInfo?.SetValue(null, value);
                    break;
                case PropertyInfo propertyInfo:
                    propertyInfo?.SetValue(null, value);
                    break;
            }
        }
        
        /// <summary>
        /// Set the value of the property/field
        /// </summary>
        internal string GetValue()
        {
            try
            {
                switch (MemberInfo)
                {
                    case FieldInfo fieldInfo:
                        return fieldInfo?.GetValue(null).ToString();
                    case PropertyInfo propertyInfo:
                        return propertyInfo?.GetValue(null).ToString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        

        /// <summary>
        /// Create a new instance of SetterInfo granting access to metadata of setter attributes
        /// </summary>
        /// <param name="declaringKey"></param>
        /// <param name="memberKey"></param>
        /// <param name="memberInfo"></param>
        /// <param name="memberInfoType"></param>
        /// <param name="isNative"></param>
        /// <param name="priority"></param>
        /// <param name="shortCut"></param>
        /// <param name="description"></param>
        /// <param name="defaultValue"></param>
        internal SetterInfo(string declaringKey, string memberKey, MemberInfo memberInfo, Type memberInfoType, bool isNative,
            int priority, [CanBeNull] string shortCut, [CanBeNull] string description, object defaultValue)
            : base(declaringKey, memberKey, memberInfo, memberInfoType, isNative, priority, shortCut, description)
        {
            this.DefaultValue = defaultValue;
        }
    }
}