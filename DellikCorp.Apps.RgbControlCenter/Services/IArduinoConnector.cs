namespace DellikCorp.Apps.RgbControlCenter.Services;

public interface IArduinoConnector
{
    Task<IEnumerable<ConnectedDevice>> FindConnectedArduino();

    Task<IEnumerable<RgbEffectMode>> GetAvailableModes(string device);

    Task<bool> ChangeModeTo(string device, int modeId);
    Task<bool> ChangeSpeedTo(string device, int speed);
    Task<bool> ChangeBrightnessTo(string device, int brightness);
    Task<bool> ChangeColorTo(string device, Color color);
}