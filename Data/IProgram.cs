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
        string Name { get; }
        int PowerConsumptionInWattHours { get; }
        int RunTimeInMinutes { get; }
        Task<ProgramStatus> GetStatusAsync();
        Task<int> GetRemainingTimeAsync();
        Task<bool> StartAsync();
        Task<bool> TryStopAsync();
    }
}
