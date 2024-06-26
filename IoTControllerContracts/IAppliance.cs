﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IAppliance : IDevice
    {
        IApplianceController Controller { get; }
        Task<IProgram[]> GetProgramsAsync();
    }
}
