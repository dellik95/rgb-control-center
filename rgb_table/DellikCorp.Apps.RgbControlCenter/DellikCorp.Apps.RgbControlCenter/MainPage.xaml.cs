using DellikCorp.Apps.RgbControlCenter.Services;
using DellikCorp.Apps.RgbControlCenter.ViewModel;

namespace DellikCorp.Apps.RgbControlCenter;

public partial class MainPage : ContentPage
{
    private MainPageViewModel viewModel = new (new ArduinoConnector());
    public MainPage()
    {

        InitializeComponent();
        this.BindingContext = viewModel;
        viewModel.LoadDevices();
    }
}

