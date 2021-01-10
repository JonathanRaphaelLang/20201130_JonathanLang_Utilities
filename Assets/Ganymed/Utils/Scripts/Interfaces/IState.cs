using System;

namespace Ganymed.Utils
{
    public interface IState
    {
        /// <summary>
        /// Set the enabled state of the object instance.
        /// </summary>
        /// <param name="enabled"></param>
        void SetEnabled(bool enabled);
        
        /// <summary>
        /// Set the active state of the object instance. Objects will only activate if they are enabled
        /// </summary>
        /// <param name="active"></param>
        void SetActive(bool active);
        
        /// <summary>
        /// Set the active and enabled state of the object instance.
        /// </summary>
        /// <param name="value"></param>
        void SetActiveAndEnabled(bool value);

        /// <summary>
        /// Returns the enabled state
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// Returns the active state
        /// </summary>
        bool IsActive { get;}
        
        /// <summary>
        /// Is the object active and enabled
        /// </summary>
        bool IsActiveAndEnabled { get; }

        //--------------------------------------------------------------------------------------------------------------
        
        event ActiveAndEnabledDelegate OnActiveAndEnabledStateChanged;
        event ActiveDelegate OnActiveStateChanged;
        event EnabledDelegate OnEnabledStateChanged;
    }
}