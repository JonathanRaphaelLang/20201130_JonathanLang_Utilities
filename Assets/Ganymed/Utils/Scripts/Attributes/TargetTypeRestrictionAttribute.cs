﻿using System;

namespace Ganymed.Utils.Attributes
{
    /// <summary>
    /// TargetTypeRestrictionAttribute limits the types to which a target attribute can be assigned.
    /// Both individual types and categories can be specified.
    /// </summary>
    [AttributesOnly]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TargetTypeRestrictionAttribute : Attribute
    {
        #region --- [PROPERTIES] ---

        /// <summary>
        /// Assembly of types that are permitted.
        /// </summary>
        public Type[] ValidTypes { get; set; }

        /// <summary>
        /// Determines if types that are derived from the transferred types are permitted.
        /// </summary>
        public bool Inherited { get; set; } = false;

        /// <summary>
        /// Flags enum containing categories of permitted types.
        /// </summary>
        public TypeAffiliations ValidTypeAffiliations
        {
            get => validTypeAffiliations;
            set
            {
                allowPrimitives = value.HasFlag(TypeAffiliations.Primitive);
                allowStrings = value.HasFlag(TypeAffiliations.String);
                allowEnums = value.HasFlag(TypeAffiliations.Enum);
                allowClass = value.HasFlag(TypeAffiliations.Class);
                allowGeneric = value.HasFlag(TypeAffiliations.Generic);
                allowInterface = value.HasFlag(TypeAffiliations.Interface);
                allowStruct = value.HasFlag(TypeAffiliations.Struct);
                validTypeAffiliations = value;
            }
        }

        #endregion

        #region --- [ALLOWED] ---
        
        /// <summary>
        /// Are primitive types valid as the type of the target property.
        /// eg. int, bool, char
        /// </summary>
        public bool AllowPrimitives
        {
            get => allowPrimitives;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.Primitive))
                    validTypeAffiliations |= TypeAffiliations.Primitive;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.Primitive))
                    validTypeAffiliations &= ~ TypeAffiliations.Primitive;

                allowPrimitives = value;
            }
        }

        /// <summary>
        /// Are strings valid as the type of the target property.
        /// </summary>
        public bool AllowStrings
        {
            get => allowStrings;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.String))
                    validTypeAffiliations |= TypeAffiliations.String;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.String))
                    validTypeAffiliations &= ~ TypeAffiliations.String;

                allowStrings = value;
            }
        }

        /// <summary>
        /// Are enums valid as the type of the target property.
        /// </summary>
        public bool AllowEnums
        {
            get => allowEnums;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.Enum))
                    validTypeAffiliations |= TypeAffiliations.Enum;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.Enum))
                    validTypeAffiliations &= ~ TypeAffiliations.Enum;

                allowEnums = value;
            }
        }
        
        /// <summary>
        /// Are structures or value types that are not valid or of type enum permitted as the type of the target property.
        /// eg. Vector3, Color32
        /// </summary>
        public bool AllowStruct
        {
            get => allowStruct;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.Struct))
                    validTypeAffiliations |= TypeAffiliations.Struct;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.Struct))
                    validTypeAffiliations &= ~ TypeAffiliations.Struct;

                allowStruct = value;
            }
        }

        /// <summary>
        /// Are classes valid as the type of the target property.
        /// </summary>
        public bool AllowClass
        {
            get => allowClass;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.Class))
                    validTypeAffiliations |= TypeAffiliations.Class;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.Class))
                    validTypeAffiliations &= ~ TypeAffiliations.Class;

                allowClass = value;
            }
        }

        /// <summary>
        /// Are generic types valid as the type of the target property.
        /// </summary>
        public bool AllowGeneric
        {
            get => allowGeneric;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.Generic))
                    validTypeAffiliations |= TypeAffiliations.Generic;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.Generic))
                    validTypeAffiliations &= ~ TypeAffiliations.Generic;

                allowGeneric = value;
            }
        }

        /// <summary>
        /// Are interfaces valid as the type of the target property.
        /// </summary>
        public bool AllowInterface
        {
            get => allowInterface;
            set
            {
                if (value && !validTypeAffiliations.HasFlag(TypeAffiliations.Interface))
                    validTypeAffiliations |= TypeAffiliations.Interface;
                else if (!value && validTypeAffiliations.HasFlag(TypeAffiliations.Interface))
                    validTypeAffiliations &= ~ TypeAffiliations.Interface;

                allowInterface = value;
            }
        }

        #endregion

        #region --- [FIELDS] ---

        private bool allowPrimitives;
        private bool allowStrings;
        private bool allowStruct;
        private bool allowEnums;
        private bool allowClass;
        private bool allowGeneric;
        private bool allowInterface;

        private TypeAffiliations validTypeAffiliations;

        #endregion

        #region --- [CONSTRUCTOR] ---

        /// <summary>
        /// Attribute allows you to define types for a target attribute to which this target attribute can be applied.
        /// Both individual types and categories can be specified.
        /// </summary>
        /// <param name="validTypes"></param>
        public TargetTypeRestrictionAttribute(params Type[] validTypes)
        {
            ValidTypes = validTypes;
        }

        #endregion
    }
}