namespace SmartPowerHub.Data
{
    public class ApplianceService
    {
        private List<IAppliance> Appliances { get; } = new List<IAppliance>();
        public Task<List<IAppliance>> GetAppliancesAsync()
        {
            return Task.FromResult(Appliances);
        }
    }
}
