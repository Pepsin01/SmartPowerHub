namespace ZigBeeControllerMockup
{
    public interface IIoTController
    {
        string Name { get; }
        Task<List<IAppliance>> GetAppliancesAsync();

        Task<IAppliance> GetApplianceAsync(int id);

        Task<bool> DeleteApplianceAsync(int id);

        Task<IAppliance> AddApplianceAsync(int id);

        Task<IAppliance> UpdateApplianceAsync(int id);

        Task<IAppliance> ConfigureApplianceAsync(string configuration);
    }
    public class Controller : IIoTController
    {
        public string Name => "ZigBee Controller";
        public Task<List<IAppliance>> GetAppliancesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IAppliance> GetApplianceAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteApplianceAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IAppliance> AddApplianceAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IAppliance> UpdateApplianceAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IAppliance> ConfigureApplianceAsync(string configuration)
        {
            throw new NotImplementedException();
        }
    }
}
