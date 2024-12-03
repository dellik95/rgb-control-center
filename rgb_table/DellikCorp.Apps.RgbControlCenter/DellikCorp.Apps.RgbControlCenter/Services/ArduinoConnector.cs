using DellikCorp.Apps.RgbControlCenter.Model;
using System.Collections.Concurrent;
using System.IO.Ports;

namespace DellikCorp.Apps.RgbControlCenter.Services
{
    internal class ArduinoConnector : IDisposable, IArduinoConnector
    {
        private readonly ConcurrentDictionary<string, SerialPort> _connections = new();

        public IEnumerable<ConnectedDevice> FindConnectedArduino()
        {
            List<ConnectedDevice> devices = new List<ConnectedDevice>();
            var ports = SerialPort.GetPortNames();

            foreach (var port in ports)
            {
                var serialPort = _connections.GetOrAdd(port, (key) => new SerialPort(key, 57600));
                using (serialPort)
                {
                    if (!serialPort.IsOpen)
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
                    }

                    serialPort.WriteLine("ping");

                    Task.Delay(400).Wait();

                    var data = serialPort.ReadLine();

                    if (data.Contains("pong"))
                    {
                        devices.Add(new ConnectedDevice()
                        {
                            Name = port,
                            Port = port
                        });
                    }
                }
            }

            return devices;
        }

        public IEnumerable<RgbEffectMode> GetAvailableModes(string device)
        {
            if (!this._connections.TryGetValue(device, out var serialPort)) yield break;
            using (serialPort)
            {
                serialPort.Open();
                serialPort.WriteLine("getModes");


                while (serialPort.IsOpen)
                {
                    var line = serialPort.ReadLine();
                    var data = line.Split("-");
                    var mode = new RgbEffectMode()
                    {
                        Id = int.Parse(data[0]),
                        Name = data[1]
                    };

                    yield return mode;
                    if (string.IsNullOrEmpty(line))
                    {
                        serialPort.Close();
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var connection in _connections)
            {
                connection.Value.Dispose();
            }
        }
    }
}
