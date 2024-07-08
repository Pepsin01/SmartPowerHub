using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTControllerContracts;

namespace ZigBeeControllerMockup
{
    public class ZigBeeSolarSourceMockup : ISolarSource
    {
        private bool _isOnline;
        private bool _isConsoleAvailable;
        private string _name;
        private string _description;
        private readonly int _maxPowerOutput;
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

        public ZigBeeSolarSourceMockup(int id, IEnergySourceController controller, string configuration = "")
        {
            _isOnline = false;
            _isConsoleAvailable = true;
            _maxPowerOutput = new Random().Next(500, 2000);


            Id = id;
            Configuration = ",,,,";
            if (!ParseConfiguration(configuration))
            {
                Name = $"Default ZigBee Energy Source {id}";
                Description = $"This is default mockup ZigBee Energy Source {id}";
                Configuration = $"ZIGBEE,{Name},{Description},port,address";
            }
            Controller = controller;
        }
        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                lock (this)
                {
                    _name = value == "" ? $"Default ZigBee Energy Source {Id}" : value;
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
                    _description = value == "" ? $"This is default mockup ZigBee Energy Source {Id}" : value;
                    UpdateConfigurationAt(2, _description);
                }
            }
        }
        public string Configuration { get; private set; }
        public IController Controller { get; }

        public Task<bool> IsOnlineAsync()
        {
            Thread.Sleep(100);
            return Task.FromResult(_isOnline);
        }

        public Task<bool> IsConsoleAvailableAsync()
        {
            Thread.Sleep(100);
            return Task.FromResult(_isConsoleAvailable);
        }

        public Task<string> SendCommandAsync(string command)
        {
            Thread.Sleep(500);
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
            Thread.Sleep(500);
            return Task.FromResult
            (
                "PORT <port> - Set the port\n" +
                "ADDRESS <address> - Set the address\n" +
                "CONNECT - Connect to the ZigBee network"
            );
        }

        public Task<double> GetMaxPowerOutput()
        {
            return Task.FromResult((double)_maxPowerOutput);
        }

        public Task<double> GetCurrentPowerOutput()
        {
            return Task.FromResult((double)new Random().Next(0, _maxPowerOutput));
        }
    }
}
