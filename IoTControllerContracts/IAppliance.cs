using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Represents an appliance that can be controlled by a controller.
    /// </summary>
    public interface IAppliance : IDevice
    {
        /// <summary>
        /// Method to get all programs that can be scheduled.
        /// </summary>
        /// <returns> An array of all programs that can be scheduled </returns>
        Task<IProgram[]> GetProgramsAsync();
    }
}
