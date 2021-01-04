using System;
using Ganymed.Utils.ExtensionMethods; // Required On Build
using UnityEngine;
using UnityEngine.EventSystems;

namespace Ganymed.Console.GUI
{
    [SelectionBase]
    public class MonoDragger : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        #region --- [FIELDS] ---

        private Vector2 lastMousePosition;
        private Vector2 mousePosition;
        private Vector2 mousePositionDifference;
        
        private Vector3 calculatedPosition;
        private Vector3 currentPosition;
        private Vector3 previousPosition;

        private RectTransform rect;

        public static event Action OnPositionChanged;

        [SerializeField] private float tolerance = 10f;
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        private void Start()
        {
            rect = GetComponent<RectTransform>();
            if (!IsRectInsideScreen(rect, tolerance))
            {
                rect.position = Vector3.zero;
            }
        }

        #region --- [DRAG BEHAVIOUR] ---

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Cursor.visible) return;
            lastMousePosition = eventData.position;
        }
        
        
        public void OnEndDrag(PointerEventData eventData)
        {
            OnPositionChanged?.Invoke();
        }
       
        public void OnDrag(PointerEventData eventData)
        {
            if (!Cursor.visible || rect == null) return;
            mousePosition = eventData.position;

            mousePositionDifference = mousePosition - lastMousePosition;

            currentPosition = rect.position;
            previousPosition = currentPosition;

            calculatedPosition = previousPosition + new Vector3(
                mousePositionDifference.x,
                mousePositionDifference.y,
                transform.position.z);

            SetRectPosition(calculatedPosition.x, calculatedPosition.y, previousPosition);
            
            lastMousePosition = mousePosition;
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [POSITION] ---

        private void SetRectPosition(Vector3 position) => rect.position = position;

        private Vector3 posX;
        private Vector3 posY;
        
        private void SetRectPosition(float X, float Y, Vector3 legacy)
        {
            posX = new Vector3(X, legacy.y, legacy.z);
            SetRectPosition(posX);

            if (!IsRectInsideScreen(rect, tolerance)) {
                SetRectPosition(legacy);
            }

            legacy = rect.position;
            
            posY = new Vector3(legacy.x, Y, legacy.z);
            SetRectPosition(posY);
            if(!IsRectInsideScreen(rect, tolerance)) {
                SetRectPosition(legacy);
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [VALIDATION] ---

        private static bool inside;
        private static Vector3[] corners;
        private static Rect displayRect;
        
        private static bool IsRectInsideScreen(RectTransform rectTransform, float tolerance)
        {
            inside = true;
            corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            displayRect = new Rect(-tolerance, -tolerance, Screen.width + tolerance * 2, Screen.height + tolerance * 2);

            foreach (var corner in corners)
                if (!displayRect.Contains(corner))
                    inside = false;
            
            return inside;
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [SCALE] ---

        

        #endregion
        
    }
}