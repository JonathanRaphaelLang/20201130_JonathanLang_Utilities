namespace Ganymed.Utils.ExtensionMethods
{
    public static class IntExtensions
    {
            
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INT] ---

        public static int Min(this int origin, int min)
            => origin < min ? min : origin;
        
        public static int Max(this int origin, int max)
            => origin > max ? max : origin;
        
        public static string Repeat(this int num, string character = " ")
        {
            var returnValue = string.Empty;
            
            for (var i = 0; i < num; i++) {
                returnValue += character;
            }

            return returnValue;
        }

        public static bool IsEven(this int n)
        {
            return (n ^ 1) == n + 1;
        }
        
        public static string AsLineHeight(this int value)
        {
            return $"<line-height={value}%>";
        }

        #endregion
    }
}
