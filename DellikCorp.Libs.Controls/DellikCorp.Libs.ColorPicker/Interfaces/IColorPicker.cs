using System.ComponentModel;


namespace DellikCorp.Libs.ColorPicker.Interfaces
{
    public interface IColorPicker : INotifyPropertyChanged
    {
        Color SelectedColor { get; set; }
        IColorPicker ConnectedColorPicker { get; set; }
    }
}
