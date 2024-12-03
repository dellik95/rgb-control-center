using Microsoft.Maui.Controls;
using System.Collections.Concurrent;
using System.IO.Ports;
using static Microsoft.Maui.ApplicationModel.Permissions;
using Microsoft.Maui.ApplicationModel;

namespace DellikCorp.Apps.RgbControlCenter.Services;

internal class ArduinoConnector : IDisposable, IArduinoConnector
{
    private readonly ConcurrentDictionary<string, SerialPort> _connections = new();

    public Task<IEnumerable<ConnectedDevice>> FindConnectedArduino()
    {
        TaskCompletionSource<IEnumerable<ConnectedDevice>> tskCompletionSource = new TaskCompletionSource<IEnumerable<ConnectedDevice>>();
        var devices = new List<ConnectedDevice>();
        var ports = SerialPort.GetPortNames();

        foreach (var port in ports)
        {
            var serialPort = _connections.GetOrAdd(port, s => new SerialPort(s, 115200));
            using (serialPort)
            {
                try
                {
                    serialPort.Open();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    continue;
                }

                serialPort.WriteLine("ping");

                var data = serialPort.ReadTo("\r\n");

                if (data.Contains("pong"))
                {
                    devices.Add(new ConnectedDevice()
                    {
                        Name = port,
                        Port = port
                    });
                }
            }

            tskCompletionSource.SetResult(devices);
        }

        return tskCompletionSource.Task;
    }

    public Task<IEnumerable<RgbEffectMode>> GetAvailableModes(string device)
    {
        var tsk = new TaskCompletionSource<IEnumerable<RgbEffectMode>>();
        var modes = new List<RgbEffectMode>();
        if (!this._connections.TryGetValue(device, out var serialPort))
        {
            tsk.SetResult(modes);
        };

        using (serialPort)
        {
            try
            {
                serialPort.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                tsk.SetResult(modes);
            }


            serialPort.WriteLine("getModes");

            var data = serialPort.ReadTo("\r\n");
            if (string.IsNullOrEmpty(data))
            {
                tsk.SetResult(modes);
            }

            var parsedModes = data.Split("\n").Select(x => x.Split("-")).Where(x => x.Length > 1).ToList();

            modes = parsedModes.Select(x => new RgbEffectMode()
            {
                Id = int.Parse(x[0]),
                Name = x[1]
            }).ToList();

            tsk.SetResult(modes);
        }

        return tsk.Task;
    }

    public Task<bool> ChangeModeTo(string device, int modeId)
    {
        var command = this.GetCommand("m", modeId);
        return this.ExecuteCommand(device, command);
    }

    public async Task<bool> ChangeSpeedTo(string device, int speed)
    {
        var command = this.GetCommand("s", speed);
        return await this.ExecuteCommand(device, command);
    }

    public async Task<bool> ChangeBrightnessTo(string device, int brightness)
    {
        var command = this.GetCommand("b", brightness);
        return await this.ExecuteCommand(device, command);
    }

    public async Task<bool> ChangeColorTo(string device, Color color)
    {
        var colorStr = color.ToHex().Replace("#", "0x");
        var command = this.GetCommand("c", colorStr);
        return await this.ExecuteCommand(device, command);
    }


    public Task<bool> ExecuteCommand(string device, string command)
    {
        TaskCompletionSource<bool> tskCompletionSource = new TaskCompletionSource<bool>();

        if (!this._connections.TryGetValue(device, out var serialPort))
        {
            tskCompletionSource.TrySetResult(false);
        };

        try
        {
            serialPort.Open();
        }
        catch (Exception)
        {
            tskCompletionSource.TrySetResult(false);
        }

        serialPort.WriteLine(command);

        void OnDataReceived(object sender, SerialDataReceivedEventArgs args)
        {
            try
            {
                var reader = sender as SerialPort;
                using (reader)
                {
                    var data = reader.ReadTo("\r\n");

                    if (!string.IsNullOrEmpty(data))
                    {
                        tskCompletionSource.TrySetResult(true);
                    }
                    reader.DataReceived -= OnDataReceived;
                }
            }
            catch (Exception e)
            {
                tskCompletionSource.TrySetResult(false);
            }
        }

        serialPort.DataReceived += OnDataReceived;

        return tskCompletionSource.Task;
    }

    private string GetCommand(string command, params object[] args)
    {
        return $"{command} {string.Join(",", args)}";
    }

    public void Dispose()
    {
        foreach (var connection in _connections)
        {
            connection.Value.Dispose();
        }
    }
}