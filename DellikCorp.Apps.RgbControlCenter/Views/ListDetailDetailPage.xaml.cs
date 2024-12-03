using DellikCorp.Libs.ColorPicker.BaseClasses.ColorPickerEventArgs;

namespace DellikCorp.Apps.RgbControlCenter.Views;

public partial class ListDetailDetailPage : ContentPage
{
    private ListDetailDetailViewModel _viewModel;
    public ListDetailDetailPage(ListDetailDetailViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = _viewModel = viewModel;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        _viewModel.LoadModes();
    }
    
    private async void ColorWheelPicker_OnDragCompleted(object sender, ColorChangedEventArgs e)
    {
        await this._viewModel.ChangeColorTo(e.NewColor);
    }
}
