using CommunityToolkit.Mvvm.ComponentModel;
using DellikCorp.Apps.RgbControlCenter.Model;
using DellikCorp.Apps.RgbControlCenter.Services;
using System.Collections.ObjectModel;

namespace DellikCorp.Apps.RgbControlCenter.ViewModel
{
    [ObservableObject]
    partial class MainPageViewModel
    {
        private readonly IArduinoConnector _arduinoConnector;

        [ObservableProperty] public ObservableCollection<ConnectedDevice> connectedItems;


        public void LoadDevices()
        {

            var devices = _arduinoConnector.FindConnectedArduino();
            this.ConnectedItems = new ObservableCollection<ConnectedDevice>(devices);
        }

        public MainPageViewModel(IArduinoConnector arduinoConnector)
        {
            _arduinoConnector = arduinoConnector;
        }
    }
}
