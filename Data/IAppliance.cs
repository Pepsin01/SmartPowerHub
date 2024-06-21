namespace SmartPowerHub.Data
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
}
