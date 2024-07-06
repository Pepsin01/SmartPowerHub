using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IBatteryController : IController
    {
        Task<IBattery?> AddDeviceAsync(int id);

        Task<IBattery?> AddDeviceWithConfigAsync(int id, string configuration);
    }
}
