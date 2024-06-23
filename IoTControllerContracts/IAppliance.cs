using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IAppliance
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        IIoTController Controller { get; }
        string Configuration { get; }
        Task<IProgram[]> GetProgramsAsync();
        Task<bool> IsOnlineAsync();
        Task<bool> IsConsoleAvailableAsync();
        Task<string> SendCommandAsync(string command);
        Task<string> GetHelp();
    }
}
