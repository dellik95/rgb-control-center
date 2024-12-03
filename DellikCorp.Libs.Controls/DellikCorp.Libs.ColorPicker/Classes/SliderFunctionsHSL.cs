﻿using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace DellikCorp.Libs.ColorPicker.Classes
{
    public static class SliderFunctionsHSL
    {
        public static float NewValueH(Color color)
        {
            return (float)color.GetHue();
        }

        public static float NewValueS(Color color)
        {
            return (float)color.GetSaturation();
        }

        public static float NewValueL(Color color)
        {
            return (float)color.GetLuminosity();
        }

        public static bool IsSelectedColorChangedH(Color color)
        {
            return true;
        }

        public static bool IsSelectedColorChangedS(Color color)
        {
            return true;
        }

        public static bool IsSelectedColorChangedL(Color color)
        {
            return true;
        }

        public static Color GetNewColorH(float newValue, Color oldColor)
        {
            return Color.FromHsla(newValue, oldColor.GetSaturation(), oldColor.GetLuminosity(), oldColor.Alpha);
        }

        public static Color GetNewColorS(float newValue, Color oldColor)
        {
            return Color.FromHsla(oldColor.GetHue(), newValue, oldColor.GetLuminosity(), oldColor.Alpha);
        }

        public static Color GetNewColorL(float newValue, Color oldColor)
        {
            return Color.FromHsla(oldColor.GetHue(), oldColor.GetSaturation(), newValue, oldColor.Alpha);
        }

        public static SKPaint GetPaintH(Color color, SKPoint startPoint, SKPoint endPoint)
        {
            var colors = new List<SKColor>();
            for (var i = 0; i <= 255; i++)
            {
                colors.Add(Color.FromHsla(i / 255D, 1, 0.5).ToSKColor());
            }

            var colorPos = new List<float>();
            for (var i = 0; i <= 255; i++)
            {
                colorPos.Add(i / 255F);
            }

            return GetPaint(colors.ToArray(), colorPos.ToArray(), startPoint, endPoint);
        }

        public static SKPaint GetPaintS(Color color, SKPoint startPoint, SKPoint endPoint)
        {
            var colors = new SKColor[]
            {
                Color.FromHsla(color.GetHue(), 0, color.GetLuminosity()).ToSKColor(),
                Color.FromHsla(color.GetHue(), 1, color.GetLuminosity()).ToSKColor()
            };

            var colorPos = new float[] { 0, 1 };
            return GetPaint(colors, colorPos, startPoint, endPoint);
        }

        public static SKPaint GetPaintL(Color color, SKPoint startPoint, SKPoint endPoint)
        {
            var colors = new SKColor[]
            {
                Color.FromHsla(color.GetHue(), color.GetSaturation(), 0).ToSKColor(),
                Color.FromHsla(color.GetHue(), color.GetSaturation(), 0.5).ToSKColor(),
                Color.FromHsla(color.GetHue(), color.GetSaturation(), 1).ToSKColor()
            };

            var colorPos = new float[] { 0, 0.5F, 1 };
            return GetPaint(colors, colorPos, startPoint, endPoint);
        }

        public static SKPaint GetPaint(SKColor[] colors, float[] colorPos, SKPoint startPoint, SKPoint endPoint)
        {
            var shader = SKShader.CreateLinearGradient(startPoint, endPoint
           , colors, colorPos, SKShaderTileMode.Clamp);

            var paint = new SKPaint()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                Shader = shader
            };
            return paint;
        }
    }
}
