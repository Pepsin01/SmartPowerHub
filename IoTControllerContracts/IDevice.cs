using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Is meant to represent a device that can be controlled by a controller.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// The id of the device. The property is set by the database.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The name of the device.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// The description of the device.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// This string wil be saved in the database and used to configure the device upon startup.
        /// </summary>
        string Configuration { get; }

        /// <summary>
        /// The controller that controls the device.
        /// </summary>
        IController Controller { get; }

        /// <summary>
        /// Checks if the device is online.
        /// </summary>
        /// <returns> True if the device is online, false otherwise </returns>
        Task<bool> IsOnlineAsync();
        /// <summary>
        /// Checks if the device is ready to receive commands.
        /// </summary>
        /// <returns> True if the device is ready to receive commands, false otherwise </returns>
        Task<bool> IsConsoleAvailableAsync();
        /// <summary>
        /// Sends a command to the device.
        /// </summary>
        /// <param name="command"> The sent command </param>
        /// <returns> The response from the device </returns>
        Task<string> SendCommandAsync(string command);

        /// <summary>
        /// This method will be called when the console for the device is evoked.
        /// </summary>
        /// <returns> The help text for controlling the device </returns>
        Task<string> GetHelp();
    }
}
