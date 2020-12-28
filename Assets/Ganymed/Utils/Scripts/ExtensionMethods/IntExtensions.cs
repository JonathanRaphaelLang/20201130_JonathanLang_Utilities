namespace Ganymed.Utils.ExtensionMethods
{
    public static class IntExtensions
    {
            
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [INT] ---

        public static int Min(this int origin, int min) => origin < min ? min : origin;
        
        public static int Max(this int origin, int max) => origin > max ? max : origin;

        public static string ToFormat(this int value)
        {
            //TODO: refactor
            var format = string.Empty;
            for (var i = 0; i < value; i++) {
                format += '0';
            }

            return format;
        }
        
                
        public static string Repeat(this int num, string character = " ")
        {
            var returnValue = string.Empty;
            
            for (var i = 0; i < num; i++) {
                returnValue += character;
            }

            return returnValue;
        }

        public static bool IsEven(this int num)
        {
            return num % 2 == 0;
        }

        #endregion
    }
}
