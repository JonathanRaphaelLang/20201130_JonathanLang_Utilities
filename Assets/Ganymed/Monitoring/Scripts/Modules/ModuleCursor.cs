using System;
using System.Threading.Tasks;
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

        [SerializeField] [HideInInspector] private string visibleColorMarkup;
        [SerializeField] [HideInInspector] private string invisibleColorMarkup;
        
        [SerializeField] [HideInInspector] private string lockedColorMarkup;
        [SerializeField] [HideInInspector] private string unlockedColorMarkup;
        [SerializeField] [HideInInspector] private string confinedColorMarkup;

        private static bool cachedVisible;
        private static CursorLockMode cachedLock;

        #endregion

        #region --- [EVENTS] ---

        public static event ModuleUpdateDelegate OnCursorStateChanged;  

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [CONSTRUCTOR] ---
        
        public ModuleCursor()
        {
            UnityEventCallbacks.AddEventListener(
                ResetCursor,
                true,
                UnityEventType.Recompile); 
        }        

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE] ---

        protected override string ParseToString(bool currentValue)
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
                           $"{Style.infixColor.AsRichText()}] " +
                           $"[{unlockedColorMarkup}Free{Style.infixColor.AsRichText()}]";
                
                case CursorLockMode.Locked:
                    return $"[{(Cursor.visible ? $"{visibleColorMarkup}Visible" : $"{invisibleColorMarkup}Hidden")}" +
                           $"{Style.infixColor.AsRichText()}] " +
                           $"[{lockedColorMarkup}Locked{Style.infixColor.AsRichText()}]";
                
                case CursorLockMode.Confined:
                    return $"[{(Cursor.visible ? $"{visibleColorMarkup}Visible" : $"{invisibleColorMarkup}Hidden")}" +
                           $"{Style.infixColor.AsRichText()}] " +
                           $"[{confinedColorMarkup}Confined{Style.infixColor.AsRichText()}]";
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnInitialize()
        {
            InitializeValue(Cursor.visible);
            InitializeUpdateEvent(ref OnCursorStateChanged);

            if (!controlCursorState || !Application.isPlaying) return;
            
            SetCursor(visibleDuringPlayMode, lockModeDuringPlayMode);
        }

        protected override void OnInspection()
        {
            if (cachedLock == Cursor.lockState && cachedVisible == Cursor.visible) return;
            cachedVisible = Cursor.visible;
            cachedLock = Cursor.lockState;
            OnCursorStateChanged?.Invoke(Cursor.visible);
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATE] ---

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!useDynamicColoring) return;
            
            visibleColorMarkup = visibleColor.AsRichText();
            invisibleColorMarkup = invisibleColor.AsRichText();

            lockedColorMarkup = lockedColor.AsRichText();
            unlockedColorMarkup = unlockedColor.AsRichText();
            confinedColorMarkup = confinedColor.AsRichText();
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [CURSOR] ---

        protected override void Tick()
        {
            if(!controlCursorState) return;
            
            if (Input.GetKeyDown(HoldKey))
            {
                SetCursor(visibleDuringHold, lockModeDuringHold);
            }
            else if(Input.GetKeyUp(HoldKey))
            {
                SetCursor(visibleDuringPlayMode, lockModeDuringPlayMode);
            }
            else if(!Application.isPlaying)
            {
                SetCursor(visibleDuringEditMode, lockModeDuringEditMode);
            }
        }

        private static void ResetCursor()
        {
            SetCursor(true, CursorLockMode.None);
        }

        private static void SetCursor(bool visible, CursorLockMode lockMode)
        {
            Cursor.visible = visible;
            Cursor.lockState = lockMode;

            cachedVisible = visible;
            cachedLock = lockMode;
            
            OnCursorStateChanged?.Invoke(Cursor.visible);
        }

        #endregion
    }
}
