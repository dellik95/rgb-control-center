using DellikCorp.Apps.RgbControlCenter.Model;

namespace DellikCorp.Apps.RgbControlCenter.Services;

public interface IArduinoConnector
{
    IEnumerable<ConnectedDevice> FindConnectedArduino();
    IEnumerable<RgbEffectMode> GetAvailableModes(string device);
}