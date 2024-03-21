namespace SmartPowerHub.Data
{
    public class Appliance
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        public List<Program> Programs { get; set; } = new List<Program>();
    }

    public struct Program
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Appliance Appliance { get; set; }
        public int PowerConsumptionInWattHours { get; set; }
        public int RunTimeInMinutes { get; set; }
    }
}
