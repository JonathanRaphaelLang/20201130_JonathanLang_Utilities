using UnityEngine;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class MarkupExtensions
    {
        #region --- [MARKUP] ---

        public static string ToRichTextMarkup(this Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>";
        }

        public static string ToFontSize(this int value)
        {
            return $"<size=#{value}>";
        } 

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
    }
}
