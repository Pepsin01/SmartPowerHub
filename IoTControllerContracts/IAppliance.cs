﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTControllerContracts
{
    public interface IAppliance : IDevice
    {
        Task<IProgram[]> GetProgramsAsync();
    }
}
