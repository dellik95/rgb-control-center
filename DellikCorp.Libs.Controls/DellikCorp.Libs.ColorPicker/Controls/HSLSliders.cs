using DellikCorp.Libs.ColorPicker.BaseClasses;
using DellikCorp.Libs.ColorPicker.Classes;

namespace DellikCorp.Libs.ColorPicker.Controls
{
    public class HSLSliders : SliderPickerWithAlpha
    {
        protected override IEnumerable<SliderBase> GetSliders()
        {
            var result = new List<BaseClasses.Slider>() {
                new(SliderFunctionsHSL.NewValueH, SliderFunctionsHSL.IsSelectedColorChangedH
                , SliderFunctionsHSL.GetNewColorH, SliderFunctionsHSL.GetPaintH),
                new (SliderFunctionsHSL.NewValueS, SliderFunctionsHSL.IsSelectedColorChangedS
                , SliderFunctionsHSL.GetNewColorS, SliderFunctionsHSL.GetPaintS),
                new (SliderFunctionsHSL.NewValueL, SliderFunctionsHSL.IsSelectedColorChangedL
                , SliderFunctionsHSL.GetNewColorL, SliderFunctionsHSL.GetPaintL)
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
