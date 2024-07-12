using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Represents a general controller for devices.
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// The name of the controller. This should be unique.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// This method will be called when device with the given id is deleted in the program.
        /// </summary>
        /// <param name="id"> The id of the device to delete </param>
        /// <returns> True if the device was deleted, false otherwise </returns>
        Task<bool> DeleteDeviceAsync(int id);
    }
}
