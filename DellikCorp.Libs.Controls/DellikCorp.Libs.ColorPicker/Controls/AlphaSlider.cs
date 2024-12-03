using DellikCorp.Libs.ColorPicker.BaseClasses;
using DellikCorp.Libs.ColorPicker.Classes;

namespace DellikCorp.Libs.ColorPicker.Controls
{
    public class AlphaSlider : SliderPicker
    {
        protected override IEnumerable<SliderBase> GetSliders()
        {
            return new SliderBase[]
            {
                new BaseClasses.Slider(SliderFunctionsAlpha.NewValueAlpha, SliderFunctionsAlpha.IsSelectedColorChangedAlpha
                , SliderFunctionsAlpha.GetNewColorAlpha, SliderFunctionsAlpha.GetPaintAlpha)
                {
                    PaintChessPattern = true
                }
            };
        }
    }
}
