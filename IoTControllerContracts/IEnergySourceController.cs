using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IEnergySourceController : IController
    {
        Task<IEnergySource?> AddDeviceAsync(int id);

        Task<IEnergySource?> AddDeviceWithConfigAsync(int id, string configuration);
    }
}
