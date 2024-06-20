namespace SmartPowerHub.Data
{
    public interface IIoTControllerInterface
    {
        Task<List<IAppliance>> GetAppliancesAsync();

        Task<IAppliance> GetApplianceAsync(int id);

        Task<bool> DeleteApplianceAsync(int id);

        Task<IAppliance> AddApplianceAsync(int id);

        Task<IAppliance> UpdateApplianceAsync(int id);

        Task<IAppliance> ConfigureApplianceAsync(string configuration);
    }
}
