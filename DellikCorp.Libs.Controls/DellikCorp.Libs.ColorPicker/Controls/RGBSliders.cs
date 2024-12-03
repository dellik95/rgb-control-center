using DellikCorp.Libs.ColorPicker.BaseClasses;
using DellikCorp.Libs.ColorPicker.Classes;

namespace DellikCorp.Libs.ColorPicker.Controls
{
    public class RGBSliders : SliderPickerWithAlpha
    {
        protected override IEnumerable<SliderBase> GetSliders()
        {
            var result = new List<BaseClasses.Slider>() {
                new (SliderFunctionsRGB.NewValueR, SliderFunctionsRGB.IsSelectedColorChanged
                , SliderFunctionsRGB.GetNewColorR, SliderFunctionsRGB.GetPaintR),
                new (SliderFunctionsRGB.NewValueG, SliderFunctionsRGB.IsSelectedColorChanged
                , SliderFunctionsRGB.GetNewColorG, SliderFunctionsRGB.GetPaintG),
                new (SliderFunctionsRGB.NewValueB, SliderFunctionsRGB.IsSelectedColorChanged
                , SliderFunctionsRGB.GetNewColorB, SliderFunctionsRGB.GetPaintB)
            };

            if (ShowAlphaSlider)
            {
                var slider = new BaseClasses.Slider(SliderFunctionsAlpha.NewValueAlpha, SliderFunctionsAlpha.IsSelectedColorChangedAlpha
                    , SliderFunctionsAlpha.GetNewColorAlpha, SliderFunctionsAlpha.GetPaintAlpha)
                {
                    PaintChessPattern = true
                };
                result.Add(slider);
            }

            return result;
        }
    }
}
