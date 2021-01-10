using System;
using Ganymed.Utils.Attributes;
using JetBrains.Annotations;

namespace Ganymed.Console.Attributes
{
    /// <summary>
    /// Base class for attributes that will expose the property/field to be issued and/or altered via console commands
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public abstract class GetterSetterAttribute : CommandBaseAttribute
    {
        #region --- [PROPERTIES] ---

        /// <summary>
        /// Adds a shortcut that makes the property/field easier to access from the console
        /// </summary>
        [CanBeNull]
        public string Shortcut
        {
            get => shortcut;
            set => shortcut = value;
        }

        #endregion

        #region --- [FIELDS] ---

        protected string description;
        protected string shortcut;
        protected int priority = 0;
        protected bool hideInBuild = false;

        #endregion
    }
}


