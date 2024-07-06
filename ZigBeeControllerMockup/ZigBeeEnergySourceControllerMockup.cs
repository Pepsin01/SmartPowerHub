using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTControllerContracts;

namespace ZigBeeControllerMockup
{
    public class ZigBeeEnergySourceControllerMockup : IEnergySourceController
    {
        private readonly List<IEnergySource> _devices = [];
        public string Name => "ZigBee Energy Source Controller";
        public Task<bool> DeleteDeviceAsync(int id)
        {
            var device = _devices.FirstOrDefault(d => d.Id == id);
            if (device == null)
                return Task.FromResult(false);
            _devices.Remove(device);
            return Task.FromResult(true);
        }

        public Task<IEnergySource?> AddDeviceAsync(int id)
        {
            var device = new ZigBeeEnergySourceMockup(id, this);
            _devices.Add(device);
            return Task.FromResult<IEnergySource?>(device);
        }

        public Task<IEnergySource?> AddDeviceWithConfigAsync(int id, string configuration)
        {
            var device = new ZigBeeEnergySourceMockup(id, this, configuration);
            _devices.Add(device);
            return Task.FromResult<IEnergySource?>(device);
        }
    }
}
