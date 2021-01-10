using System;
using System.Reflection;
using JetBrains.Annotations;

namespace Ganymed.Console.Processor
{
    /// <summary>
    /// base for classes granting access to metadata of getter and setter attributes
    /// </summary>
    internal abstract class GetterSetterInfo
    {
        #region --- [PROPERTIES] ---

        /// <summary>
        /// The name of the declaring type. Can be altered from original name
        /// </summary>
        internal readonly string DeclaringKey;
        
        /// <summary>
        /// The name of the member. Can be altered from original name
        /// </summary>
        internal readonly string MemberKey;
        
        /// <summary>
        /// The type of the MemberInfo (PropertyInfo or FieldInfo)
        /// </summary>
        internal readonly Type MemberInfoType;
        
        /// <summary>
        /// MemberInfo class of the stored Getter/Setter
        /// </summary>
        internal readonly MemberInfo MemberInfo;
        
        
        /// <summary>
        /// Is the Getter/Setter required by native operations
        /// </summary>
        internal readonly bool IsNative;

        /// <summary> 
        /// Getter/Setter with a higher priority will be prefered by autocompletion and are shown higher up in listings. 
        /// </summary>
        internal readonly int Priority;
        
        /// <summary>
        /// Is MemberInfoType of type PropertyInfo
        /// </summary>
        internal readonly bool IsProperty;
        
        /// <summary>
        /// Is MemberInfoType of type FieldInfo
        /// </summary>
        internal readonly bool IsField;
        
        
        /// <summary>
        /// Shortcut to access the Getter/Setter. Can be null
        /// </summary>
        [CanBeNull] internal readonly string ShortCut;
        
        /// <summary>
        /// Custom description of the Getter/Setter. Can be null
        /// </summary>
        [CanBeNull] internal readonly string Description;
        
        /// <summary>
        /// Type of the Property/Field
        /// </summary>
        internal readonly Type ValueType;

        #endregion

        internal GetterSetterInfo(string declaringKey, string memberKey, MemberInfo memberInfo, Type memberInfoType,
            bool isNative, int priority, [CanBeNull] string shortCut, [CanBeNull] string description)
        {
            this.DeclaringKey = declaringKey;
            this.MemberKey = memberKey;
            this.MemberInfo = memberInfo;
            this.MemberInfoType = memberInfoType;
            this.IsNative = isNative;
            this.Priority = priority;
            this.ShortCut = shortCut;
            this.Description = description;
            
            
            if (MemberInfoType == typeof(PropertyInfo))
            {
                ValueType = (MemberInfo as PropertyInfo)?.PropertyType;
                IsProperty = true;
            }
                
            else if (MemberInfoType == typeof(FieldInfo))
            {
                ValueType = (MemberInfo as FieldInfo)?.FieldType;
                IsField = true;
            }
        }
    }
}
