using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IEnergySource : IDevice
    {
        IEnergySourceController EnergySourceController { get; }
        Task<int> GetMaxPowerOutput();
        Task<int> GetCurrentPowerOutput();
    }
}
