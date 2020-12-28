using System;
using UnityEngine;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class ColorExtensions
    {
        public static Color ToUnityEngineColor(this ColorEnum target)
        {
            switch (target)
            {
                case ColorEnum.White:
                    return Color.white;
                
                case ColorEnum.LightGrey:
                    return new Color(0.56f, 0.6f, 0.7f);
                
                case ColorEnum.Grey:
                    return new Color(0.38f, 0.41f, 0.5f);
                
                case ColorEnum.DarkGrey:
                    return new Color(0.21f, 0.21f, 0.27f);
                
                case ColorEnum.Black:
                    return Color.black;
                    
                case ColorEnum.Yellow:
                    return new Color(1f, 0.95f, 0.21f);
                    
                case ColorEnum.Orange:
                    return new Color(1f, 0.67f, 0.18f);
                    
                case ColorEnum.Brown:
                    return new Color(0.54f, 0.35f, 0.12f);
                    
                case ColorEnum.Red:
                    return new Color(1f, 0.04f, 0f);
                    
                case ColorEnum.Bordeaux:
                    return new Color(1f, 0.04f, 0.54f);
                    
                case ColorEnum.Pink:
                    return new Color(0.93f, 0.55f, 1f);
                    
                case ColorEnum.Magenta:
                    return new Color(1f, 0f, 0.9f);
                    
                case ColorEnum.Violette:
                    return new Color(0.38f, 0.17f, 1f);
                    
                case ColorEnum.Blue:
                    return new Color(0.22f, 0.1f, 1f);
                    
                case ColorEnum.Navy:
                    return new Color(0.34f, 0.33f, 1f);
                    
                case ColorEnum.LightBlue:
                    return new Color(0.45f, 0.59f, 1f);
                    
                case ColorEnum.Cyan:
                    return new Color(0f, 0.6f, 1f);
                
                case ColorEnum.Turquoise:
                    return new Color(0f, 1f, 0.84f);
                
                case ColorEnum.Green:
                    return new Color(0.16f, 0.7f, 0.38f);
                
                case ColorEnum.Lime:
                    return new Color(0f, 1f, 0.21f);

                case ColorEnum.X:
                    return new Color(0.91f, 0.22f, 0.24f);
                
                case ColorEnum.Y:
                    return new Color(0.44f, 1f, 0.31f);
                
                case ColorEnum.Z:
                    return new Color(0.35f, 0.55f, 1f);
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }
    }
}