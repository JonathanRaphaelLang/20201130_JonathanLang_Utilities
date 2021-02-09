using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ganymed.Console.GUIElements
{
    public class ButtonScaler : MonoBehaviour
    {
        #region --- [INSPECTOR && FIELDS] ---

        [Header("Reference")]
        [SerializeField] private RectTransform rect = null;
        [SerializeField] private Image image = null;
        [SerializeField] private Sprite iconFullSize = null;
        [SerializeField] private Sprite iconNotFullSize = null;
        [SerializeField] private Image backgroundImage = null;
        [SerializeField] private TextMeshProUGUI textField = null;
        [SerializeField] private Canvas canvas = null;
        
        private Vector2 cachedPosition = Vector2.zero;
        private float fallbackWidth = 500;
        private float fallbackHeight = 200;
        private Sprite cachedBackgroundSprite = null;
        private float scaleFactor = default;
        
        #endregion
    
        //--------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            scaleFactor = canvas.scaleFactor;
        }

        #region --- [SCALE] ---
    
        private void OnEnable()
        {
            MonoScaler.OnScaleChanged += OnScaleChanged;
            MonoDragger.OnPositionChanged += OnScaleChanged;
        }

        private void OnDisable()
        {
            MonoScaler.OnScaleChanged -= OnScaleChanged;
            MonoDragger.OnPositionChanged -= OnScaleChanged;
        }
    
        private void OnScaleChanged()
        {
            image.sprite = iconFullSize;
            image.SetNativeSize();
            cachedPosition = rect.position;

            var sizeDelta = rect.sizeDelta;
            fallbackHeight = sizeDelta.y;
            fallbackWidth = sizeDelta.x;
        }
    
        //--------------------------------------------------------------------------------------------------------------

        private float lerp = default;
        
        public async void SetSize()
        {
            if (!Core.ConsoleSettings.Instance.allowAnimations)
            {
                textField.enabled = false;
                if (!IsFullSize())
                {
                    var target = new Vector2(Screen.width / scaleFactor, Screen.height / scaleFactor);
                    SetImage(iconNotFullSize);
                    rect.sizeDelta = target;
                    rect.position = Vector2.zero;
                    cachedBackgroundSprite = backgroundImage.sprite;
                    backgroundImage.sprite = null;
                }
                else
                {
                    var target = new Vector2(fallbackWidth, fallbackHeight); 
                    SetImage(iconFullSize);
                    rect.sizeDelta = target;
                    rect.position = cachedPosition;
                    backgroundImage.sprite = cachedBackgroundSprite;
                }
                textField.enabled = true;
                return;
            }
            
            textField.enabled = false;
            
            lerp = .015f;
            
            if (!IsFullSize())
            {
                var target = new Vector2(Screen.width / scaleFactor, Screen.height / scaleFactor);
                SetImage(iconNotFullSize);
                
                
                var timer = Time.realtimeSinceStartup + .1f;
                while (Time.realtimeSinceStartup < timer)
                {
                    rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, target, lerp );
                    rect.position = Vector2.Lerp(rect.position, Vector2.zero, lerp );
                    lerp = Mathf.Lerp(lerp, .33f, lerp) + Time.deltaTime;
                    await Task.Delay(5);
                }
                
                rect.sizeDelta = target;
                rect.position = Vector2.zero;

                cachedBackgroundSprite = backgroundImage.sprite;
                backgroundImage.sprite = null;
            }
            else
            {
                var target = new Vector2(fallbackWidth, fallbackHeight); 
                SetImage(iconFullSize);

                var timer = Time.realtimeSinceStartup + .1f;
                while (Time.realtimeSinceStartup < timer)
                {
                    rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, target, lerp);
                    rect.position = Vector2.Lerp(rect.position, cachedPosition, lerp);
                    lerp = Mathf.Lerp(lerp, .33f, lerp) + Time.deltaTime;;
                    await Task.Delay(5);
                }
                
                rect.sizeDelta = target;
                rect.position = cachedPosition;
            
                backgroundImage.sprite = cachedBackgroundSprite;
            }
            
            textField.enabled = true;
        }
        

        private bool IsFullSize()
        {
            scaleFactor = canvas.scaleFactor;
            return rect.sizeDelta == new Vector2(Screen.width / scaleFactor, Screen.height / scaleFactor);
        }

        private void SetImage(Sprite sprite)
        {
            image.sprite = sprite;
            image.SetNativeSize();
        }

        #endregion
    }
}
