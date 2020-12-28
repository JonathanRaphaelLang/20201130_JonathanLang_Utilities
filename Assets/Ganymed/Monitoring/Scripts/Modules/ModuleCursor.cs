using System;
using Ganymed.Monitoring.Core;
using Ganymed.Utils;
using Ganymed.Utils.Callbacks;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_Cursor", menuName = "Monitoring/Modules/Cursor")]
    public sealed class ModuleCursor : Module<bool>
    {

#if UTILS_CALLBACKS
 private bool test = false;
#endif
        
        #region --- [INSPECTOR] ---

        [Space] [Header("Cursor")] [SerializeField]
        private bool controlCursorState = false;

        [Header("PlayMode")] [SerializeField] private bool visibleDuringPlayMode = true;
        [SerializeField] private CursorLockMode lockModeDuringPlayMode = CursorLockMode.None;

        [Header("EditMode")] [SerializeField] private bool visibleDuringEditMode = true;
        [SerializeField] private CursorLockMode lockModeDuringEditMode = CursorLockMode.None;

        [Header("Hold")] [SerializeField] private KeyCode HoldKey = KeyCode.LeftAlt;
        [SerializeField] private bool visibleDuringHold = true;
        [SerializeField] private CursorLockMode lockModeDuringHold = CursorLockMode.None;

        [Header("GUI")]
        [SerializeField] private bool useDynamicColoring = true;
        [Space]
        [SerializeField] private Color visibleColor = Color.cyan;
        [SerializeField] private Color invisibleColor = Color.cyan;
        [Space]
        [SerializeField] private Color lockedColor = Color.cyan;
        [SerializeField] private Color unlockedColor = Color.cyan;
        [SerializeField] private Color confinedColor = Color.cyan;

        #endregion

        #region --- [FIELDS] ---

        private string visibleColorMarkup;
        private string invisibleColorMarkup;
        
        private string lockedColorMarkup;
        private string unlockedColorMarkup;
        private string confinedColorMarkup;

        private static bool cachedVisible;
        private static CursorLockMode cachedLock;

        #endregion

        #region --- [EVENTS] ---

        public static event Action<bool> OnCursorStateChanged;  

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---
        
        public ModuleCursor()
        {
            UnityEventCallbacks.AddEventListener(
                ValidateCursor,
                true,
                CallbackDuring.PlayMode,
                UnityEventType.Update);
            
            UnityEventCallbacks.AddEventListener(
                ResetCursor,
                true,
                UnityEventType.Recompile); 
        }        

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE] ---

        protected override string DynamicValue(bool value)
        {
            if (!useDynamicColoring)
            {
                return $"[{(Cursor.visible ? "Visible" : "Hidden")}] " +
                       $"[{(Cursor.lockState == CursorLockMode.None? "Free" : Cursor.lockState.ToString())}]";    
            }
            //else 
            switch (Cursor.lockState)
            {
                case CursorLockMode.None:
                    return $"[{(Cursor.visible ? $"{visibleColorMarkup}Visible" : $"{invisibleColorMarkup}Hidden")}" +
                           $"{Config.infixColor.ToRichTextMarkup()}] " +
                           $"[{unlockedColorMarkup}Free{Config.infixColor.ToRichTextMarkup()}]";
                
                case CursorLockMode.Locked:
                    return $"[{(Cursor.visible ? $"{visibleColorMarkup}Visible" : $"{invisibleColorMarkup}Hidden")}" +
                           $"{Config.infixColor.ToRichTextMarkup()}] " +
                           $"[{lockedColorMarkup}Locked{Config.infixColor.ToRichTextMarkup()}]";
                
                case CursorLockMode.Confined:
                    return $"[{(Cursor.visible ? $"{visibleColorMarkup}Visible" : $"{invisibleColorMarkup}Hidden")}" +
                           $"{Config.infixColor.ToRichTextMarkup()}] " +
                           $"[{confinedColorMarkup}Confined{Config.infixColor.ToRichTextMarkup()}]";
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnInitialize()
        {
            SetValueDelegate(ref OnCursorStateChanged);

            if (controlCursorState && Application.isPlaying)
            {
                SetCursor(visibleDuringPlayMode, lockModeDuringPlayMode);
                OnCursorStateChanged?.Invoke(Cursor.visible);
            }
            
            OnCursorStateChanged?.Invoke(Cursor.visible);
        }

        protected override void OnInspection()
        {
            if (cachedLock != Cursor.lockState || cachedVisible != Cursor.visible)
            {
                cachedVisible = Cursor.visible;
                cachedLock = Cursor.lockState;
                OnCursorStateChanged?.Invoke(Cursor.visible);
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATE] ---

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!useDynamicColoring) return;
            
            visibleColorMarkup = visibleColor.ToRichTextMarkup();
            invisibleColorMarkup = invisibleColor.ToRichTextMarkup();

            lockedColorMarkup = lockedColor.ToRichTextMarkup();
            unlockedColorMarkup = unlockedColor.ToRichTextMarkup();
            confinedColorMarkup = confinedColor.ToRichTextMarkup();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CURSOR] ---

        private void ValidateCursor()
        {
            if(!controlCursorState) return;
            
            if (Input.GetKeyDown(HoldKey))
            {
                SetCursor(visibleDuringHold, lockModeDuringHold);
                OnCursorStateChanged?.Invoke(Cursor.visible);
            }
            else if(Input.GetKeyUp(HoldKey))
            {
                SetCursor(visibleDuringPlayMode, lockModeDuringPlayMode);
                OnCursorStateChanged?.Invoke(Cursor.visible);
            }
            else if(!Application.isPlaying)
            {
                SetCursor(visibleDuringEditMode, lockModeDuringEditMode);
                OnCursorStateChanged?.Invoke(Cursor.visible);
            }
        }

        private static void ResetCursor()
        {
            SetCursor(true, CursorLockMode.None);
            OnCursorStateChanged?.Invoke(Cursor.visible);
        }

        private static void SetCursor(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;

            cachedVisible = visible;
            cachedLock = lockMode;
        }

        #endregion
    }
}
