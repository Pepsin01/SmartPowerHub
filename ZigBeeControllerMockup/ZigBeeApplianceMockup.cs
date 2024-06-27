// Ignore Spelling: Mockup

using IoTControllerContracts;

namespace ZigBeeControllerMockup
{
    internal class ZigBeeApplianceMockup : IAppliance
    {
        private bool _isOnline;
        private bool _isConsoleAvailable;
        private string _name;
        private string _description;
        private readonly List<IProgram> _programs;
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

        private List<IProgram> InitializeProgramsRandom(int id)
        {
            int index = (id - 1) % 3;

            (int lowerBoundry, int upperBoundry)[] possibleAppliancePowers = {
                (1, 2), // testing appliance
                (500, 1500), // something like a washing machine
                (5000, 15000) // something like a heat pump
            };
            (int lowerBoundry, int upperBoundry)[] possibleApplianceRunTimes = {
                (1, 2), // testing appliance
                (60, 120), // something like a washing machine
                (240, 720) // something like a heat pump
            };
            string[] possibleProgramNames = {
                "Normal",
                "Eco",
                "Intensive",
            };

            var random = new Random();
            return (from possibleProgramName in possibleProgramNames
                    let powerConsumption =
                        random.Next(possibleAppliancePowers[index].lowerBoundry, possibleAppliancePowers[index].upperBoundry)
                    let runTime =
                        random.Next(possibleApplianceRunTimes[index].lowerBoundry,
                            possibleApplianceRunTimes[index].upperBoundry)
                    select new ZigBeeProgramMockup(possibleProgramName, powerConsumption, runTime, this))
                .Cast<IProgram>()
                .ToList();
        }

        public ZigBeeApplianceMockup(int id, IApplianceController controller, string configuration = "")
        {
            _isOnline = false;
            _isConsoleAvailable = true;
            _programs = InitializeProgramsRandom(id);

            Id = id;
            Configuration = ",,,,";
            if (!ParseConfiguration(configuration))
            {
                Name = $"Default ZigBee Appliance {id}";
                Description = $"This is default mockup ZigBee Appliance {id}";
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
        public IApplianceController Controller { get; }
        public string Configuration { get; private set; }
        public Task<IProgram[]> GetProgramsAsync()
        {
            return Task.FromResult(_programs.ToArray());
        }

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
            return Task.FromResult
            (
                """
                PORT <port> - Set the port
                ADDRESS <address> - Set the address
                CONNECT - Connect to the ZigBee network

                """
            );
        }

        public void SetAvailable()
        {
            lock (this)
            {
                foreach (var program in _programs.Where(program =>
                             program.GetStatusAsync().Result != ProgramStatus.Running))
                {
                    if (program is ZigBeeProgramMockup zigBeeProgram)
                    {
                        zigBeeProgram.MakeAvailable();
                    }
                }
            }
        }

        public void SetUnavailable()
        {
            lock (this)
            {
                foreach (var program in _programs.Where(program =>
                             program.GetStatusAsync().Result != ProgramStatus.Running))
                {
                    if (program is ZigBeeProgramMockup zigBeeProgram)
                    {
                        zigBeeProgram.MakeUnavailable();
                    }
                }
            }
        }
    }
}
