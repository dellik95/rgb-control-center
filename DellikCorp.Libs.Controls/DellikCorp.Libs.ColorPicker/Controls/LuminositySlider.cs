using DellikCorp.Libs.ColorPicker.BaseClasses;
using DellikCorp.Libs.ColorPicker.Classes;

namespace DellikCorp.Libs.ColorPicker.Controls
{
    public class LuminositySlider : SliderPicker
    {
        protected override IEnumerable<SliderBase> GetSliders()
        {
            return new SliderBase[]
            {
                new BaseClasses.Slider(SliderFunctionsHSL.NewValueL, SliderFunctionsHSL.IsSelectedColorChangedL
                , SliderFunctionsHSL.GetNewColorL, SliderFunctionsHSL.GetPaintL)
            };
        }
    }
}
