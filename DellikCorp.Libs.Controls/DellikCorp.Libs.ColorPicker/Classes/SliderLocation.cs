using DellikCorp.Libs.ColorPicker.BaseClasses;
using SkiaSharp;

namespace DellikCorp.Libs.ColorPicker.Classes
{
    public class SliderLocation
    {
        public SliderLocation(SliderBase slider)
        {
            Slider = slider;
        }
        public SliderBase Slider { get; private set; }
        public long? LocationProgressId { get; set; }
        public SKPoint Location { get; set; } = new();
        public float OffsetLocationMultiplier { get; set; }
        public float GetSliderOffset(float PickerRadiusPixels)
        {
            return PickerRadiusPixels * OffsetLocationMultiplier;
        }
    }
}
