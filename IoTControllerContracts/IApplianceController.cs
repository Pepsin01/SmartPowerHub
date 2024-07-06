namespace IoTControllerContracts
{
    public interface IApplianceController : IController
    {
        Task<IAppliance?> AddDeviceAsync(int id);

        Task<IAppliance?> AddDeviceWithConfigAsync(int id, string configuration);
    }
}
