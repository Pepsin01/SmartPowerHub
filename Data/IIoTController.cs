namespace SmartPowerHub.Data
{
    public interface IIoTController
    {
        string Name { get; }
        Task<IAppliance?> GetApplianceAsync(int id);

        Task<bool> DeleteApplianceAsync(int id);

        Task<IAppliance?> AddApplianceAsync(int id);

        Task<IAppliance?> AddApplianceWithConfigAsync(int id, string configuration);
    }
}
