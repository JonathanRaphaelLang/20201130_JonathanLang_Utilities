using Ganymed.Utils.ExtensionMethods;

namespace Ganymed.Utils.ColorTables
{
    /// <summary>
    /// Class containing a variety of preprocessed color
    /// </summary>
    public static class CS
    {
        public const string Clear = "</color>";
        
        public static string Red => red ?? (red = Paint.red.AsRichText());
        private static string red;
        
        public static string Green => green ?? (green = Paint.green.AsRichText());
        private static string green;
        
        public static string Blue => blue ?? (blue = Paint.blue.AsRichText());
        private static string blue;
        
        
        public static string Cyan => cyan ?? (cyan = Paint.cyan.AsRichText());
        private static string cyan;
        
        public static string Magenta => magenta ?? (magenta = Paint.magenta.AsRichText());
        private static string magenta;
        
        public static string Yellow => yellow ?? (yellow = Paint.yellow.AsRichText());
        private static string yellow;
        
        public static string Black => black ?? (black = Paint.black.AsRichText());
        private static string black;
        
        
        public static string Orange => orange ?? (orange = Paint.orange.AsRichText());
        private static string orange;
        
        public static string Violet => violet ?? (violet = Paint.violet.AsRichText());
        private static string violet;
        
        
        public static string White => white ?? (white = Paint.white.AsRichText());
        private static string white;
        
        public static string LightGray => lightGray ?? (lightGray = Paint.lightGray.AsRichText());
        private static string lightGray;
        
        public static string Gray => gray ?? (gray = Paint.gray.AsRichText());
        private static string gray;
        
        public static string DarkGray => darkGray ?? (darkGray = Paint.darkGray.AsRichText());
        private static string darkGray;
    }
}
