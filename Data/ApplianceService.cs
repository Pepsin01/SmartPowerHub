namespace SmartPowerHub.Data
{
    public class ApplianceService
    {
        private static readonly List<Appliance> Appliances = new List<Appliance>
        {
            new Appliance
            {
                Id = 1,
                Name = "Fridge",
                Description = "A fridge",
                IsRunning = false,
                Programs = new List<Program>
                {
                    new Program
                    {
                        Id = 1,
                        Name = "Normal",
                        PowerConsumptionInWattHours = 100,
                        RunTimeInMinutes = 60
                    },
                    new Program
                    {
                        Id = 2,
                        Name = "Eco",
                        PowerConsumptionInWattHours = 50,
                        RunTimeInMinutes = 60
                    }
                }
            },
            new Appliance
            {
                Id = 2,
                Name = "Washing Machine",
                Description = "A washing machine",
                IsRunning = false,
                Programs = new List<Program>
                {
                    new Program
                    {
                        Id = 3,
                        Name = "Normal",
                        PowerConsumptionInWattHours = 200,
                        RunTimeInMinutes = 120
                    },
                    new Program
                    {
                        Id = 4,
                        Name = "Eco",
                        PowerConsumptionInWattHours = 100,
                        RunTimeInMinutes = 120
                    }
                }
            }
        };
        public Task<List<Appliance>> GetAppliancesAsync()
        {
            return Task.FromResult(Appliances);
        }
    }
}
