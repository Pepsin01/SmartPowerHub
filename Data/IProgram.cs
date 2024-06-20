namespace SmartPowerHub.Data
{
    public enum ProgramStatus
    {
        Available,
        Unavailable,
        Running
    }
    public interface IProgram
    {
        int Id { get; set; }
        string Name { get; set; }
        int PowerConsumptionInWattHours { get; set; }
        int RunTimeInMinutes { get; set; }
        Task<ProgramStatus> GetStatusAsync();
        Task<bool> StartAsync();
    }
}
