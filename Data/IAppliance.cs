namespace SmartPowerHub.Data
{
    public interface IAppliance
    {
        int Id { get; set; }
        string Name { get; set; }
        string Description { get; set; }
        List<IProgram> Programs { get; set; }
        Task<bool> IsOnlineAsync();
    }
}
