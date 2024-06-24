using Microsoft.EntityFrameworkCore;
using SmartPowerHub.Database.Contexts;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Database
{
    public class SeedData
    {
        public static void SeedMockAppliances(ApplianceContext context)
        {
            if (context.Appliances.Any())
                return;

            var appliances = new List<ApplianceModel>
            {

            };
            foreach (var appliance in appliances)
                context.Appliances.Add(appliance);
        }
    }
}
