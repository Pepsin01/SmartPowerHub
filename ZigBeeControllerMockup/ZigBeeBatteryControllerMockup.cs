using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTControllerContracts;

namespace ZigBeeControllerMockup
{
    public class ZigBeeBatteryControllerMockup : IBatteryController
    {
        private readonly List<IBattery> _batteries = [];
        public string Name => "ZigBee Battery Controller";
        public Task<bool> DeleteDeviceAsync(int id)
        {
            var battery = _batteries.FirstOrDefault(d => d.Id == id);
            if (battery == null)
                return Task.FromResult(false);
            _batteries.Remove(battery);
            return Task.FromResult(true);
        }

        public Task<IBattery?> AddDeviceAsync(int id)
        {
            var battery = new ZigBeeBatteryMockup(id, this);
            _batteries.Add(battery);
            return Task.FromResult<IBattery?>(battery);
        }

        public Task<IBattery?> AddDeviceWithConfigAsync(int id, string configuration)
        {
            var battery = new ZigBeeBatteryMockup(id, this, configuration);
            _batteries.Add(battery);
            return Task.FromResult<IBattery?>(battery);
        }
    }
}
