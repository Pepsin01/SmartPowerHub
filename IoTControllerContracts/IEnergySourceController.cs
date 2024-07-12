﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Represents an energy source controller that can control energy sources.
    /// </summary>
    public interface IEnergySourceController : IController
    {
        /// <summary>
        /// Adds an energy source to the controller.
        /// </summary>
        /// <param name="id"> The id generated by the database for the energy source </param>
        /// <returns> The added energy source if successful, null otherwise </returns>
        Task<IEnergySource?> AddDeviceAsync(int id);

        /// <summary>
        /// Adds an energy source to the controller with a configuration.
        /// </summary>
        /// <param name="id"> The id generated by the database for the energy source </param>
        /// <param name="configuration"> The configuration for the energy source </param>
        /// <returns> The added energy source if successful, null otherwise </returns>
        Task<IEnergySource?> AddDeviceWithConfigAsync(int id, string configuration);
    }
}
