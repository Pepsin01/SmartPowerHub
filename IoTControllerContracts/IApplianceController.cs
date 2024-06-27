namespace IoTControllerContracts
{
    public interface IApplianceController
    {
        string Name { get; }
        Task<IAppliance?> GetApplianceAsync(int id);

        Task<bool> DeleteApplianceAsync(int id);

        Task<IAppliance?> AddApplianceAsync(int id);

        Task<IAppliance?> AddApplianceWithConfigAsync(int id, string configuration);
    }
}
