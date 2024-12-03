namespace DellikCorp.Apps.RgbControlCenter.ViewModels;

[QueryProperty(nameof(Device), nameof(Device))]
public partial class ListDetailDetailViewModel : BaseViewModel
{
    private readonly IArduinoConnector _arduinoConnector;

    public ListDetailDetailViewModel(IArduinoConnector arduinoConnector)
    {
        _arduinoConnector = arduinoConnector;
        this.InitializeColors();
    }

    [ObservableProperty] 
    Color selectedColor = new (230, 16,50, 0);

    [ObservableProperty]
    ConnectedDevice device;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    int selectedSpeed;

    [ObservableProperty]
    int selectedBrightness;

    [ObservableProperty]
    ObservableCollection<RgbEffectMode> modes;

    [ObservableProperty] 
    byte redColorChanel;    
    
    [ObservableProperty] 
    byte greenColorChanel;    
    
    [ObservableProperty] 
    byte blueColorChanel;


    [RelayCommand]
    public async void LoadModes()
    {
        this.Modes = new ObservableCollection<RgbEffectMode>(await this._arduinoConnector.GetAvailableModes(Device.Port));
    }

    [RelayCommand]
    private void OnRefreshing()
    {
        IsRefreshing = true;

        try
        {
            LoadModes();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task ApplyMode(RgbEffectMode effect)
    {
        await this._arduinoConnector.ChangeModeTo(this.Device.Port, effect.Id);
    }

    [RelayCommand]
    private async Task SpeedSelected()
    {
        await this._arduinoConnector.ChangeSpeedTo(this.Device.Port, this.SelectedSpeed);
    }

    [RelayCommand]
    private async Task BrightnessSelected()
    {
        await this._arduinoConnector.ChangeBrightnessTo(this.Device.Port, this.SelectedBrightness);
    }

    public async Task ChangeColorTo(Color color)
    {
        this.InitializeColors();
        await this._arduinoConnector.ChangeColorTo(this.Device.Port, color);
    }

    [RelayCommand]
    private async Task ColorChanelSelected()
    {
        this.SelectedColor = new Color(this.RedColorChanel, this.GreenColorChanel, this.BlueColorChanel);
        await this.ChangeColorTo(this.SelectedColor);
    }


    private void InitializeColors()
    {
        this.selectedColor.ToRgb(out var red, out var green, out var blue);
        this.RedColorChanel = red;
        this.GreenColorChanel = green;
        this.BlueColorChanel = blue;
    }
}
