using System.Threading.Tasks;
using Ganymed.Console.GUI;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScaler : MonoBehaviour
{
    #region --- [INSPECTOR && FIELDS] ---

    [Space]
    [SerializeField] private RectTransform rect = null;
    [SerializeField] private Image image = null;
    [SerializeField] private Sprite iconFullSize = null;
    [SerializeField] private Sprite iconNotFullSize = null;
    [SerializeField] private Image backgroundImage = null;
    
    private Canvas canvas;
    private Vector2 cachedPosition = Vector2.zero;
    private float fallbackWidth = 500;
    private float fallbackHeight = 200;
    private Sprite cachedBackgroundSprite = null;

    #endregion
    
    //--------------------------------------------------------------------------------------------------------------
        
    #region --- [SCALE] ---
    
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        MonoScaler.OnScaleChanged += OnMono;
        MonoDragger.OnPositionChanged += OnMono;
    }

    private void OnDisable()
    {
        MonoScaler.OnScaleChanged -= OnMono;
        MonoDragger.OnPositionChanged -= OnMono;
    }
    
    private void OnMono()
    {
        image.sprite = iconFullSize;
        image.SetNativeSize();
        cachedPosition = rect.position;

        var sizeDelta = rect.sizeDelta;
        fallbackHeight = sizeDelta.y;
        fallbackWidth = sizeDelta.x;
    }
    
    //--------------------------------------------------------------------------------------------------------------

    public async void SetSize()
    {
        if (!IsFullSize())
        {
            var scaleFactor = canvas.scaleFactor;
            var target = new Vector2(Screen.width / scaleFactor, Screen.height / scaleFactor);
            SetImage(iconNotFullSize);

            for (var i = 0; i < 100; i++)
            {
                rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, target, .05f);
                rect.position = Vector2.Lerp(rect.position, Vector2.zero, .05f);
                await Task.Delay(2);
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
            
            for (var i = 0; i < 100; i++)
            {
                rect.sizeDelta = Vector2.Lerp(rect.sizeDelta, target, .05f);
                rect.position = Vector2.Lerp(rect.position, cachedPosition, .05f);
                await Task.Delay(2);
            }
            rect.sizeDelta = target;
            rect.position = cachedPosition;
            
            backgroundImage.sprite = cachedBackgroundSprite;
        }
    }

    private bool IsFullSize()
    {
        var scaleFactor = canvas.scaleFactor;
        return rect.sizeDelta == new Vector2(Screen.width / scaleFactor, Screen.height / scaleFactor);
    }

    private void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
        image.SetNativeSize();
    }

    #endregion
}
