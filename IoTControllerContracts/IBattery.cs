using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IBattery : IDevice
    {
        IBatteryController BatteryController { get; }
        Task<int> GetCapacityAsync();
        Task<int> GetChargeLevelAsync();
    }
}
