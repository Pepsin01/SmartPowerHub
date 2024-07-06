// Ignore Spelling: Mockup

using IoTControllerContracts;

namespace ZigBeeControllerMockup
{
    public class ZigBeeApplianceControllerMockup : IApplianceController
    {
        private readonly List<IAppliance> _appliances = new ();
        public string Name => "ZigBee Controller";

        public Task<IAppliance?> GetDeviceAsync(int id)
        {
            return Task.FromResult(_appliances.FirstOrDefault(appliance => appliance.Id == id));
        }

        public Task<bool> DeleteDeviceAsync(int id)
        {
            var appliance = _appliances.FirstOrDefault(appliance => appliance.Id == id);
            if (appliance == null)
                return Task.FromResult(false);
            _appliances.Remove(appliance);
            return Task.FromResult(true);
        }

        public Task<IAppliance?> AddDeviceAsync(int id)
        {
            var appliance = new ZigBeeApplianceMockup(id, this);
            _appliances.Add(appliance);
            return Task.FromResult<IAppliance?>(appliance);
        }

        public Task<IAppliance?> AddDeviceWithConfigAsync(int id, string configuration)
        {
            var appliance = new ZigBeeApplianceMockup(id, this, configuration);
            _appliances.Add(appliance);
            return Task.FromResult<IAppliance?>(appliance);
        }
    }
}
