using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public enum ProgramStatus
    {
        Available,
        Unavailable,
        Running
    }
    public interface IProgram
    {
        string Name { get; }
        double PowerConsumptionInWattHours { get; }
        int RunTimeInMinutes { get; }
        IAppliance Appliance { get; }
        Task<ProgramStatus> GetStatusAsync();
        Task<int> GetRemainingTimeAsync();
        Task<bool> StartAsync();
        Task<bool> TryStopAsync();
    }
}
