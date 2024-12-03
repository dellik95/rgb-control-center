using DellikCorp.Libs.ColorPicker.BaseClasses;
using DellikCorp.Libs.ColorPicker.BaseClasses.ColorPickerEventArgs;
using DellikCorp.Libs.ColorPicker.Classes;
using DellikCorp.Libs.ColorPicker.Interfaces;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;

namespace DellikCorp.Libs.ColorPicker;

public partial class ColorWheelPicker : ContentView, IColorPicker
{
    private long? _locationHsProgressId;
    private long? _locationLProgressId;
    private SKPoint _locationHs;
    private SKPoint _locationL;
    private readonly SKColor[] _sweepGradientColors = new SKColor[256];

    public ColorWheelPicker()
    {
        InitializeComponent();
        HorizontalOptions = LayoutOptions.Center;
        VerticalOptions = LayoutOptions.Center;

        for (var i = 128; i >= -127; i--)
        {
            _sweepGradientColors[255 - (i + 127)] = Color.FromHsla((i < 0 ? 255 + i : i) / 255D, 1, 0.5).ToSKColor();
        }
    }

    #region Bindable Properties

    public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
      nameof(SelectedColor),
      typeof(Color),
      typeof(IColorPicker),
      Color.FromHsla(0, 0, 0.5),
      propertyChanged: HandleSelectedColorSet);

    static void HandleSelectedColorSet(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != newValue)
        {
            ((ColorWheelPicker)bindable).ChangeSelectedColor((Color)newValue);
            if (((ColorWheelPicker)bindable).ConnectedColorPicker != null)
            {
                ((ColorWheelPicker)bindable).ConnectedColorPicker.SelectedColor = (Color)newValue;
            }
            ((ColorWheelPicker)bindable).RaiseSelectedColorChanged((Color)oldValue, (Color)newValue);
        }
    }

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly BindableProperty ConnectedColorPickerProperty = BindableProperty.Create(
       nameof(ConnectedColorPicker),
       typeof(IColorPicker),
       typeof(IColorPicker),
       propertyChanged: HandleConnectedColorPickerSet);

    static void HandleConnectedColorPickerSet(BindableObject bindable, object oldValue, object newValue)
    {
        if (oldValue != null)
        {
            ((IColorPicker)oldValue).PropertyChanged -= ((ColorWheelPicker)bindable).BindedIColorPicker_PropertyChanged;
        }
        if (newValue != null)
        {
            ((IColorPicker)newValue).PropertyChanged += ((ColorWheelPicker)bindable).BindedIColorPicker_PropertyChanged;
            ((IColorPicker)newValue).SelectedColor = ((ColorWheelPicker)bindable).SelectedColor;
        }
    }

    public IColorPicker ConnectedColorPicker
    {
        get => (IColorPicker)GetValue(ConnectedColorPickerProperty);
        set => SetValue(ConnectedColorPickerProperty, value);
    }

    private void BindedIColorPicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedColor))
        {
            SelectedColor = ((IColorPicker)sender).SelectedColor;
        }
    }

    public static readonly BindableProperty ShowLuminosityWheelProperty = BindableProperty.Create(
        nameof(ShowLuminosityWheel),
        typeof(bool),
        typeof(SkiaSharpPickerBase),
        true,
        propertyChanged: HandleShowLuminositySet);

    static void HandleShowLuminositySet(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            ((ColorWheelPicker)bindable).InvalidateSurface();
        }
    }

    public bool ShowLuminosityWheel
    {
        get => (bool)GetValue(ShowLuminosityWheelProperty);
        set => SetValue(ShowLuminosityWheelProperty, value);
    }

    public static readonly BindableProperty WheelBackgroundColorProperty = BindableProperty.Create(
        nameof(WheelBackgroundColor),
        typeof(Color),
        typeof(IColorPicker),
        Colors.Transparent,
        propertyChanged: HandleWheelBackgroundColorSet);

    static void HandleWheelBackgroundColorSet(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue)
        {
            ((ColorWheelPicker)bindable).InvalidateSurface();
        }
    }

    public Color WheelBackgroundColor
    {
        get => (Color)GetValue(WheelBackgroundColorProperty);
        set => SetValue(WheelBackgroundColorProperty, value);
    }

    public static readonly BindableProperty PickerRadiusScaleProperty = BindableProperty.Create(
        nameof(PickerRadiusScale),
        typeof(float),
        typeof(SkiaSharpPickerBase),
        0.05F,
        propertyChanged: HandlePickerRadiusScaleSet);

    static void HandlePickerRadiusScaleSet(BindableObject bindable, object oldValue, object newValue)
    {
        ((ColorWheelPicker)bindable).InvalidateSurface();
    }

    public float PickerRadiusScale
    {
        get => (float)GetValue(PickerRadiusScaleProperty);
        set => SetValue(PickerRadiusScaleProperty, value);
    }

    #endregion
    public event EventHandler<ColorChangedEventArgs> SelectedColorChanged;

    public event EventHandler<ColorChangedEventArgs> DragCompleted;

    protected virtual void RaiseSelectedColorChanged(Color oldColor, Color newColor)
    {
        SelectedColorChanged?.Invoke(this, new ColorChangedEventArgs(oldColor, newColor));
    }

    protected virtual void RaiseDragCompleted()
    {
        DragCompleted?.Invoke(this, new ColorChangedEventArgs(null, this.SelectedColor));
    }

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
        CanvasView.InvalidateSurface();
    }

    protected SKSize GetCanvasSize()
    {
        return CanvasView.CanvasSize;
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

    private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
    }

    public float GetPickerRadiusPixels()
    {
        return GetPickerRadiusPixels(GetCanvasSize());
    }

    public float GetPickerRadiusPixels(SKSize canvasSize)
    {
        return GetSize(canvasSize) * PickerRadiusScale;
    }

    protected void OnPaintSurface(SKCanvas canvas, int width, int height)
    {
        var canvasRadius = GetSize() / 2F;
        UpdateLocations(SelectedColor, canvasRadius);
        canvas.Clear();
        PaintBackground(canvas, canvasRadius);
        if (ShowLuminosityWheel)
        {
            PaintLGradient(canvas, canvasRadius);
            PaintPicker(canvas, _locationL);
        }
        PaintColorSweepGradient(canvas, canvasRadius);
        PaintGrayRadialGradient(canvas, canvasRadius);
        PaintPicker(canvas, _locationHs);
    }

    protected void ChangeSelectedColor(Color color)
    {
        InvalidateSurface();
    }

    protected SizeRequest GetMeasure(double widthConstraint, double heightConstraint)
    {
        if (Double.IsPositiveInfinity(widthConstraint) &&
            Double.IsPositiveInfinity(heightConstraint))
        {
            widthConstraint = 200;
            heightConstraint = 200;
        }

        var size = Math.Min(widthConstraint, heightConstraint);

        return new SizeRequest(new Size(size, size));
    }

    protected float GetSize()
    {
        return GetSize(GetCanvasSize());
    }

    protected float GetSize(SKSize canvasSize)
    {
        return canvasSize.Width;
    }

    private void UpdateLocations(Color color, float canvasRadius)
    {
        if (color.GetLuminosity() != 0 || !IsInHsArea(_locationHs, canvasRadius))
        {
            var angleHS = (0.5 - color.GetHue()) * (2 * Math.PI);
            var radiusHS = WheelHsRadius(canvasRadius) * color.GetSaturation();

            var resultHS = FromPolar(new PolarPoint((float)radiusHS, (float)angleHS));
            resultHS.X += canvasRadius;
            resultHS.Y += canvasRadius;
            _locationHs = resultHS;
        }

        var polarL = ToPolar(ToWheelLCoordinates(_locationL, canvasRadius));
        polarL.Angle -= (float)Math.PI / 2F;
        var signOld = polarL.Angle <= 0 ? 1 : -1;
        var angleL = color.GetLuminosity() * Math.PI * signOld;

        var resultL = FromPolar(new PolarPoint(WheelLRadius(canvasRadius), (float)(angleL - (Math.PI / 2))));
        resultL.X += canvasRadius;
        resultL.Y += canvasRadius;
        _locationL = resultL;
    }

    private void UpdateColors(float canvasRadius)
    {
        var wheelHSPoint = ToWheelHSCoordinates(_locationHs, canvasRadius);
        var wheelLPoint = ToWheelLCoordinates(_locationL, canvasRadius);
        var newColor = WheelPointToColor(wheelHSPoint, wheelLPoint);
        SelectedColor = newColor;
    }

    private bool IsInHsArea(SKPoint point, float canvasRadius)
    {
        var polar = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        return polar.Radius <= WheelHsRadius(canvasRadius);
    }

    private bool IsInLArea(SKPoint point, float canvasRadius)
    {
        if (!ShowLuminosityWheel)
        {
            return false;
        }
        var polar = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        return polar.Radius <= WheelLRadius(canvasRadius) + (GetPickerRadiusPixels() / 2F)
            && polar.Radius >= WheelLRadius(canvasRadius) - (GetPickerRadiusPixels() / 2F);
    }

    private void PaintBackground(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Color = WheelBackgroundColor.ToSKColor()
        };

        canvas.DrawCircle(center, canvasRadius - GetPickerRadiusPixels(), paint);
    }

    private void PaintLGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var colors = new List<SKColor>()
            {
                Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor(),
                Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 1).ToSKColor(),
                Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor(),
                Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0).ToSKColor(),
                Color.FromHsla(SelectedColor.GetHue(), SelectedColor.GetSaturation(), 0.5).ToSKColor()
            };

        var shader = SKShader.CreateSweepGradient(center, colors.ToArray(), null);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = shader,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = GetPickerRadiusPixels()
        };
        canvas.DrawCircle(center, WheelLRadius(canvasRadius), paint);
    }

    private void PaintColorSweepGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var shader = SKShader.CreateSweepGradient(center, _sweepGradientColors, null);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = shader,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(center, WheelHsRadius(canvasRadius), paint);
    }

    private void PaintGrayRadialGradient(SKCanvas canvas, float canvasRadius)
    {
        var center = new SKPoint(canvasRadius, canvasRadius);

        var colors = new SKColor[] {
                SKColors.Gray,
                SKColors.Transparent
            };

        var shader = SKShader.CreateRadialGradient(center, WheelHsRadius(canvasRadius), colors, null, SKShaderTileMode.Clamp);

        var paint = new SKPaint
        {
            IsAntialias = true,
            Shader = shader,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawPaint(paint);
    }

    private SKPoint ToWheelHSCoordinates(SKPoint point, float canvasRadius)
    {
        var result = new SKPoint(point.X, point.Y);
        result.X -= canvasRadius;
        result.Y -= canvasRadius;
        result.X /= WheelHsRadius(canvasRadius);
        result.Y /= WheelHsRadius(canvasRadius);
        return result;
    }

    private SKPoint ToWheelLCoordinates(SKPoint point, float canvasRadius)
    {
        var result = new SKPoint(point.X, point.Y);
        result.X -= canvasRadius;
        result.Y -= canvasRadius;
        result.X /= WheelLRadius(canvasRadius);
        result.Y /= WheelLRadius(canvasRadius);
        return result;
    }

    private Color WheelPointToColor(SKPoint pointHS, SKPoint pointL)
    {
        var polarHS = ToPolar(pointHS);
        var polarL = ToPolar(pointL);
        polarL.Angle += (float)Math.PI / 2F;
        polarL = ToPolar(FromPolar(polarL));
        var h = (Math.PI - polarHS.Angle) / (2 * Math.PI);
        var s = polarHS.Radius;
        var l = Math.Abs(polarL.Angle) / Math.PI;
        return Color.FromHsla(h, s, l, SelectedColor.Alpha);
    }

    private SKPoint LimitToHSRadius(SKPoint point, float canvasRadius)
    {
        var polar = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        polar.Radius = polar.Radius < WheelHsRadius(canvasRadius) ? polar.Radius : WheelHsRadius(canvasRadius);
        var result = FromPolar(polar);
        result.X += canvasRadius;
        result.Y += canvasRadius;
        return result;
    }

    private SKPoint LimitToLRadius(SKPoint point, float canvasRadius)
    {
        var polar = ToPolar(new SKPoint(point.X - canvasRadius, point.Y - canvasRadius));
        polar.Radius = WheelLRadius(canvasRadius);
        var result = FromPolar(polar);
        result.X += canvasRadius;
        result.Y += canvasRadius;
        return result;
    }

    private PolarPoint ToPolar(SKPoint point)
    {
        var radius = (float)Math.Sqrt((point.X * point.X) + (point.Y * point.Y));
        var angle = (float)Math.Atan2(point.Y, point.X);
        return new PolarPoint(radius, angle);
    }

    private SKPoint FromPolar(PolarPoint point)
    {
        var x = (float)(point.Radius * Math.Cos(point.Angle));
        var y = (float)(point.Radius * Math.Sin(point.Angle));
        return new SKPoint(x, y);
    }

    private float WheelHsRadius(float canvasRadius)
    {
        return !ShowLuminosityWheel ? canvasRadius - GetPickerRadiusPixels() : canvasRadius - (3 * GetPickerRadiusPixels()) - 2;
    }

    private float WheelLRadius(float canvasRadius)
    {
        return canvasRadius - GetPickerRadiusPixels();
    }

    private void CanvasView_OnTouch(object sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                OnTouchActionPressed(e);
                break;
            case SKTouchAction.Moved:
                OnTouchActionMoved(e);
                break;
            case SKTouchAction.Released:
                OnTouchActionReleased(e);
                break;
            case SKTouchAction.Cancelled:
                OnTouchActionCancelled(e);
                break;
        }
    }

    private void OnTouchActionPressed(SKTouchEventArgs args)
    {
        var canvasRadius = GetCanvasSize().Width / 2F;
        var point = args.Location;
        if (_locationHsProgressId == null && IsInHsArea(point, canvasRadius))
        {
            _locationHsProgressId = args.Id;
            _locationHs = LimitToHSRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        else if (_locationLProgressId == null && IsInLArea(point, canvasRadius))
        {
            _locationLProgressId = args.Id;
            _locationL = LimitToLRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
    }

    private void OnTouchActionMoved(SKTouchEventArgs args)
    {
        var canvasRadius = GetCanvasSize().Width / 2F;
        var point = args.Location;
        if (_locationHsProgressId == args.Id)
        {
            _locationHs = LimitToHSRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        else if (_locationLProgressId == args.Id)
        {
            _locationL = LimitToLRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
    }

    private void OnTouchActionReleased(SKTouchEventArgs args)
    {
        var canvasRadius = GetCanvasSize().Width / 2F;
        var point = args.Location;
        if (_locationHsProgressId == args.Id)
        {
            _locationHsProgressId = null;
            _locationHs = LimitToHSRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        else if (_locationLProgressId == args.Id)
        {
            _locationLProgressId = null;
            _locationL = LimitToLRadius(point, canvasRadius);
            UpdateColors(canvasRadius);
        }
        this.RaiseDragCompleted();
    }

    private void OnTouchActionCancelled(SKTouchEventArgs args)
    {
        if (_locationHsProgressId == args.Id)
        {
            _locationHsProgressId = null;
        }
        else if (_locationLProgressId == args.Id)
        {
            _locationLProgressId = null;
        }
    }
}