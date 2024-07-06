using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IController
    {
        string Name { get; }
        Task<bool> DeleteDeviceAsync(int id);
    }
}
