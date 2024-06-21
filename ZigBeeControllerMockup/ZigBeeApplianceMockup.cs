// Ignore Spelling: Mockup

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZigBeeControllerMockup
{
    public interface IAppliance
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        string ControllerName { get; }
        string Configuration { get; }
        List<IProgram> Programs { get; set; }
        Task<bool> IsOnlineAsync();
        Task<bool> IsConsoleAvailableAsync();
        Task<string> SendCommandAsync(string command);
        Task<string> GetHelp();
    }
    internal class ZigBeeApplianceMockup : IAppliance
    {
        private bool _isOnline;
        private bool _isConsoleAvailable;
        private string _name;
        private string _description;

        private bool ParseConfiguration(string configuration)
        {
            if (configuration == "")
                return false;
            if (!configuration.StartsWith("ZIGBEE"))
                return false;
            
            var parsedConfiguration = configuration.Split(',');

            if (parsedConfiguration.Length < 5)
                return false;

            Name = parsedConfiguration[1];
            Description = parsedConfiguration[2];

            Configuration = configuration;
            return true;
        }

        private void UpdateConfigurationAt(int index, string newPart)
        {
            var splitConfiguration = Configuration.Split(',');
            splitConfiguration[index] = newPart;
            Configuration = string.Join(',', splitConfiguration);
        }

        public ZigBeeApplianceMockup(int id, string controllerName, string configuration = "")
        {
            _isOnline = false;
            _isConsoleAvailable = true;

            Id = id;
            if (!ParseConfiguration(configuration))
            {
                Name = $"Default ZigBee Appliance {id}";
                Description = $"This is default mockup ZigBee Appliance {id}";
                Configuration = $"ZIGBEE,{Name},{Description},port,address";
            }
            ControllerName = controllerName;
        }
        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                lock (this)
                {
                    _name = value == "" ? $"Default ZigBee Appliance {Id}" : value;
                    UpdateConfigurationAt(1, _name);
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                lock (this)
                {
                    _description = value == "" ? $"This is default mockup ZigBee Appliance {Id}" : value;
                    UpdateConfigurationAt(2, _description);
                }
            }
        }
        public string ControllerName { get; }
        public string Configuration { get; private set; }
        public List<IProgram> Programs { get; set; }
        public Task<bool> IsOnlineAsync()
        {
            return Task.FromResult(_isOnline);
        }

        public Task<bool> IsConsoleAvailableAsync()
        {
            return Task.FromResult(_isConsoleAvailable);
        }

        public Task<string> SendCommandAsync(string command)
        {
            var parsedCommand = command.Split(' ');

            if (parsedCommand.Length < 1)
                return Task.FromResult("ERROR: No command specified");

            switch (parsedCommand[0])
            {
                case "PORT":
                    if (parsedCommand.Length < 2)
                        return Task.FromResult("ERROR: No port specified");
                    lock (this)
                        UpdateConfigurationAt(3, parsedCommand[1]);
                    return Task.FromResult("OK");
                case "ADDRESS":
                    if (parsedCommand.Length < 3)
                        return Task.FromResult("ERROR: No address specified");
                    lock (this)
                        UpdateConfigurationAt(4, parsedCommand[2]);
                    return Task.FromResult("OK");
                case "CONNECT":
                    _isConsoleAvailable = false;
                    lock (this)
                    {
                        Thread.Sleep(2000);
                        _isOnline = true;
                    }
                    _isConsoleAvailable = true;
                    return Task.FromResult("OK");
                default:
                    return Task.FromResult("ERROR: Unknown command");
            }
        }

        public Task<string> GetHelp()
        {
            return Task.FromResult("PORT <port> - Set the port\nADDRESS <address> - Set the address\nCONNECT - Connect to the ZigBee network\n");
        }
    }
}
