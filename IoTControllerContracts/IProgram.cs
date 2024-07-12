using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    /// <summary>
    /// Represents a status of a program.
    /// </summary>
    public enum ProgramStatus
    {
        Available, // The program is available to be started
        Unavailable, // The program is not available to be started
        Running // The program is currently running
    }

    /// <summary>
    /// Represents a program that can be scheduled on an appliance.
    /// </summary>
    public interface IProgram
    {
        /// <summary>
        /// Gets the name of the program.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets the power consumption of the program in watt-hours.
        /// </summary>
        double PowerConsumptionInWattHours { get; }
        /// <summary>
        /// Gets the run time of the program in minutes.
        /// </summary>
        int RunTimeInMinutes { get; }
        /// <summary>
        /// Gets the programs appliance.
        /// </summary>
        IAppliance Appliance { get; }
        /// <summary>
        /// Gets the status of the program.
        /// </summary>
        /// <returns> The status of the program </returns>
        Task<ProgramStatus> GetStatusAsync();

        /// <summary>
        /// Gets the remaining time of the running program in minutes.
        /// </summary>
        /// <returns> The remaining time of the running program in minutes </returns>
        Task<int> GetRemainingTimeAsync();
        /// <summary>
        /// Starts the program.
        /// </summary>
        /// <returns> True if the program was started, false otherwise </returns>
        Task<bool> StartAsync();
        /// <summary>
        /// Tries to stop the program.
        /// </summary>
        /// <returns> True if the program was stopped, false otherwise </returns>
        Task<bool> TryStopAsync();
    }
}
