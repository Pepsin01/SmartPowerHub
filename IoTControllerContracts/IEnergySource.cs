using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IEnergySource : IDevice
    {
        Task<int> GetMaxPowerOutput();
        Task<int> GetCurrentPowerOutput();
    }
}
