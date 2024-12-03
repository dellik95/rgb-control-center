using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DellikCorp.Apps.RgbControlCenter.Services
{
    internal class FakeArduinoConnector : IArduinoConnector
    {
        public Task<IEnumerable<ConnectedDevice>> FindConnectedArduino()
        {
            return Task.FromResult(Enumerable.Range(0, 10).Select(x => new ConnectedDevice()
            {
                Name = $"RGB {x}",
                Port = $"COM {x}"
            }));
        }

        public Task<IEnumerable<RgbEffectMode>> GetAvailableModes(string device)
        {
            return Task.FromResult(Enumerable.Range(0, 100).Select(x => new RgbEffectMode()
            {
                Name = $"Mode {x}",
                Id = x
            }));
        }

        public Task<bool> ChangeModeTo(string device, int modeId)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangeSpeedTo(string device, int speed)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangeBrightnessTo(string device, int brightness)
        {
            return Task.FromResult(true);
        }

        public Task<bool> ChangeColorTo(string device, Color color)
        {
            return Task.FromResult(true);
        }
    }
}
