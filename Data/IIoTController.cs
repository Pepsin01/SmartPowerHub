namespace SmartPowerHub.Data
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
}
