using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Represents an energy source that can be controlled by a controller.
    /// </summary>
    public interface IEnergySource : IDevice
    {
        /// <summary>
        /// Gets the maximum power output of the energy source.
        /// </summary>
        /// <returns> The maximum power output of the energy source </returns>
        Task<double> GetMaxPowerOutput();
        /// <summary>
        /// Gets the current power output of the energy source.
        /// </summary>
        /// <returns> The current power output of the energy source </returns>
        Task<double> GetCurrentPowerOutput();
    }
}
