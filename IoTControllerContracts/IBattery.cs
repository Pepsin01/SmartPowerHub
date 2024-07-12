using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Represents a battery that can be controlled by a controller.
    /// </summary>
    public interface IBattery : IDevice
    {
        /// <summary>
        /// Gets the capacity of the battery.
        /// </summary>
        /// <returns> The capacity of the battery </returns>
        Task<double> GetCapacityAsync();

        /// <summary>
        /// Gets the charge level of the battery.
        /// </summary>
        /// <returns> The charge level of the battery </returns>
        Task<double> GetChargeLevelAsync();
    }
}
