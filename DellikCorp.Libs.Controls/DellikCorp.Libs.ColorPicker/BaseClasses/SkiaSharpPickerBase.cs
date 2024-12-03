using DellikCorp.Libs.ColorPicker.Effects;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

namespace DellikCorp.Libs.ColorPicker.BaseClasses
{
    public abstract class SkiaSharpPickerBase : ColorPickerViewBase
    {
        protected readonly View CanvasView;

        protected SkiaSharpPickerBase()
        {
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;

            if (DeviceInfo.Platform == DevicePlatform.WinUI && DeviceInfo.Idiom == DeviceIdiom.Phone)
            {
                var view = new SKCanvasView();
                view.PaintSurface += CanvasView_PaintSurface;
                CanvasView = view;
            }
            else
            {
                var view = new SKGLView();
                view.PaintSurface += GLView_PaintSurface;
                CanvasView = view;
            }

            var touchEffect = new ColorPickerTouchEffect()
            {
                Capture = true
            };

            touchEffect.TouchAction += TouchEffect_TouchAction;
            Effects.Add(touchEffect);
            this.Children.Add(CanvasView);
        }

        public static readonly BindableProperty PickerRadiusScaleProperty = BindableProperty.Create(
           nameof(PickerRadiusScale),
           typeof(float),
           typeof(SkiaSharpPickerBase),
           0.05F,
           propertyChanged: HandlePickerRadiusScaleSet);

        static void HandlePickerRadiusScaleSet(BindableObject bindable, object oldValue, object newValue)
        {
            ((SkiaSharpPickerBase)bindable).InvalidateSurface();
        }

        public float PickerRadiusScale
        {
            get
            {
                return (float)GetValue(PickerRadiusScaleProperty);
            }
            set
            {
                SetValue(PickerRadiusScaleProperty, value);
            }
        }

        public abstract float GetPickerRadiusPixels();
        public abstract float GetPickerRadiusPixels(SKSize canvasSize);

        protected abstract SizeRequest GetMeasure(double widthConstraint, double heightConstraint);
        protected abstract float GetSize();
        protected abstract float GetSize(SKSize canvasSize);
        protected abstract void OnPaintSurface(SKCanvas canvas, int width, int height);
        protected abstract void OnTouchActionPressed(ColorPickerTouchActionEventArgs args);
        protected abstract void OnTouchActionMoved(ColorPickerTouchActionEventArgs args);
        protected abstract void OnTouchActionReleased(ColorPickerTouchActionEventArgs args);
        protected abstract void OnTouchActionCancelled(ColorPickerTouchActionEventArgs args);

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            return GetMeasure(widthConstraint, heightConstraint);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            CanvasView.Layout(new Rect(x, y, width, height));
        }

        protected SKPoint ConvertToPixel(Point pt)
        {
            var canvasSize = GetCanvasSize();
            return new SKPoint((float)(canvasSize.Width * pt.X / CanvasView.Width),
                               (float)(canvasSize.Height * pt.Y / CanvasView.Height));
        }

        protected void InvalidateSurface()
        {
            switch (CanvasView)
            {
                case SKCanvasView view:
                    view.InvalidateSurface();
                    return;
                case SKGLView view:
                    view.InvalidateSurface();
                    break;
            }
        }

        protected SKSize GetCanvasSize()
        {
            return CanvasView switch
            {
                SKCanvasView view => view.CanvasSize,
                SKGLView view => view.CanvasSize,
                _ => SKSize.Empty
            };
        }

        protected void PaintPicker(SKCanvas canvas, SKPoint point)
        {
            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            paint.Color = Colors.White.ToSKColor();
            paint.StrokeWidth = 2;
            canvas.DrawCircle(point, GetPickerRadiusPixels() - 2, paint);

            paint.Color = Colors.Black.ToSKColor();
            paint.StrokeWidth = 1;
            canvas.DrawCircle(point, GetPickerRadiusPixels() - 4, paint);
            canvas.DrawCircle(point, GetPickerRadiusPixels(), paint);
        }

        private void CanvasView_PaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }

        private void GLView_PaintSurface(object? sender, SKPaintGLSurfaceEventArgs e)
        {
            OnPaintSurface(e.Surface.Canvas, e.BackendRenderTarget.Width, e.BackendRenderTarget.Height);
        }

        private void TouchEffect_TouchAction(object sender, ColorPickerTouchActionEventArgs e)
        {
            switch (e.Type)
            {
                case ColorPickerTouchActionType.Pressed:
                    OnTouchActionPressed(e);
                    break;
                case ColorPickerTouchActionType.Moved:
                    OnTouchActionMoved(e);
                    break;
                case ColorPickerTouchActionType.Released:
                    OnTouchActionReleased(e);
                    break;
                case ColorPickerTouchActionType.Cancelled:
                    OnTouchActionCancelled(e);
                    break;
            }
        }
    }
}
