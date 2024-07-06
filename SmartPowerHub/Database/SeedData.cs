using Microsoft.EntityFrameworkCore;
using SmartPowerHub.Database.Contexts;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Database
{
    public class SeedData
    {
        public static void SeedMockAppliances(DeviceContext context)
        {
            if (context.Devices.Any())
                return;

            var appliances = new List<DeviceModel>
            {

            };
            foreach (var appliance in appliances)
                context.Devices.Add(appliance);
        }
    }
}
