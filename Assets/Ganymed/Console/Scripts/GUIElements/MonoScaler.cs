using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ganymed.Console.GUIElements
{
    public class MonoScaler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region --- [INSPECTOR] ---

        [Header("Scale")]
        [SerializeField] private RectTransform rect = null;
        [SerializeField] [Range(50,200)] private float minSizeX = 100f;
        [SerializeField] [Range(50,200)] private float minSizeY = 100f;
        [SerializeField] private float tolerance = 5f;
        [Space]
        [SerializeField] private TextMeshProUGUI ConsoleTextMesh = null;

        #endregion

        #region --- [FIELDS] ---
    
        private Vector2 lastMousePosition;
        private Canvas canvas;
        private Vector2 posX;
        private Vector2 posY;
        
        private bool disableOnScale = true;

        #endregion

        #region --- [EVENTS] ---

        public static event Action OnScaleChanged;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SUBSCRIPTIONS] ---

        private void OnEnable() => Core.Console.OnConsoleRenderSettingsUpdateCallback += UpdateRenderSettings;
        private void OnDisable() => Core.Console.OnConsoleRenderSettingsUpdateCallback -= UpdateRenderSettings;
        private void OnDestroy() => Core.Console.OnConsoleRenderSettingsUpdateCallback -= UpdateRenderSettings;

        private void UpdateRenderSettings(bool renderContentOnDrag, bool renderContentOnScale)
            => disableOnScale = !renderContentOnScale;

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [DRAG] ---

        private void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            lastMousePosition = eventData.position;
            if (disableOnScale) ConsoleTextMesh.enabled = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var currentMousePosition = eventData.position;
            var calculated = (currentMousePosition - lastMousePosition) / canvas.scaleFactor;

            SetRectScale(calculated.x, calculated.y);

            lastMousePosition = currentMousePosition;
        }

        private void SetRectScale(float X, float Y)
        {
            rect.sizeDelta += new Vector2(X,0);

            if (!IsRectInsideScreen(rect, tolerance) || rect.sizeDelta.x < minSizeX ) {
                rect.sizeDelta -= new Vector2(X,0);
            }
            
            rect.sizeDelta += new Vector2(0,Y);
        
            if(!IsRectInsideScreen(rect, tolerance) || rect.sizeDelta.y < minSizeY ) {
                rect.sizeDelta -= new Vector2(0,Y);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnScaleChanged?.Invoke();
            if (disableOnScale) ConsoleTextMesh.enabled = true;
        }
   

        private static bool IsRectInsideScreen(RectTransform rectTransform, float tolerance)
        {
            var inside = true;
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            var displayRect = new Rect(
                    -tolerance,
                    -tolerance,
                    Screen.width + tolerance * 2,
                    Screen.height +  tolerance * 2);

            foreach (var corner in corners)
                if (!displayRect.Contains(corner))
                    inside = false;
            
            return inside;
        }
        
        #endregion
    }
}
