using UnityEngine;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class MarkupExtensions
    {
        #region --- [MARKUP] ---
        
        public static string ToRichTextFontSize(this int num)
        {
            return $"<size={num.Min(0)}>";
        }

        public static string AsRichText(this Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        }
        public static string AsRichText(this Color32 color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        }
        
        public static string ToRichTextMarker(this Color color)
        {
            return $"<mark=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        }

        public static string ToFontSize(this float value, float? defaultSize = null)
        {
            var calculatedSize = defaultSize / 100 * value;
            return $"<size=#{calculatedSize ?? value}>";
        }
        

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
    }
}
