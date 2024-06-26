using System.Reflection;
using System.Text;
using IoTControllerContracts;
using Serilog;
using SmartPowerHub.Database.Contexts;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Data;

public class ApplianceService
{
    private readonly List<IIoTController> _controllers;
    private readonly List<IAppliance> _appliances;
    private readonly IServiceProvider _serviceProvider;

    public ApplianceService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _controllers = InitializeAvailableControllers(Path.Combine(Environment.CurrentDirectory, "IoTControllers"));
        _appliances = InitializeAppliances();
    }

    /// <summary>
    ///     Initializes all available controllers from the specified path
    /// </summary>
    /// <param name="path"> The path to the directory containing the controller dlls </param>
    /// <returns> A list of all found controllers </returns>
    private List<IIoTController> InitializeAvailableControllers(string path)
    {
        Log.Information($"Searching for controllers in {path}");
        var controllers = new List<IIoTController>();
        var files = Directory.GetFiles(path, "*.dll");
        foreach (var file in files)
            try
            {
                Log.Information($"Found controller dll: {file}");
                var assembly = Assembly.LoadFrom(file);
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.GetInterface(nameof(IIoTController)) == null) continue;
                    if (Activator.CreateInstance(type) is not IIoTController controller) continue;
                    controllers.Add(controller);
                    Log.Information($"Found controller: {controller.Name}");
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var sb = new StringBuilder();
                foreach (var exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    if (exSub is FileNotFoundException exFileNotFound)
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }

                    sb.AppendLine();
                }

                var errorMessage = sb.ToString();

                Log.Error(errorMessage);
            }

        return controllers;
    }

    /// <summary>
    ///    Initializes all appliances from the database
    /// </summary>
    /// <returns> A list of appliances initialized from the database </returns>
    private List<IAppliance> InitializeAppliances()
    {
        Log.Information("Initializing appliances from database");

        // Get all appliance records from the database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplianceContext>();
        var applianceRecords = context.Appliances.ToList();
        var appliances = new List<IAppliance>();

        // Add all appliances to the controllers
        foreach (var record in applianceRecords)
        {
            var appliance = _controllers.FirstOrDefault(c => c.Name == record.ControllerName)
                ?.AddApplianceWithConfigAsync(record.Id, record.Configuration).Result;
            if (appliance != null)
                appliances.Add(appliance);
        }

        return appliances;
    }

    /// <summary>
    ///   Gets all currently added appliances
    /// </summary>
    /// <returns> A list of all currently added appliances </returns>
    public Task<IAppliance[]> GetAppliancesAsync()
    {
        return Task.FromResult(_appliances.ToArray());
    }

    /// <summary>
    ///    Adds a new appliance to the database and the controller
    /// </summary>
    /// <param name="controller"> The controller to add the appliance to </param>
    /// <returns> The added appliance or null if the appliance could not be added </returns>
    public async Task<IAppliance?> AddApplianceAsync(string controllerName)
    {
        var controller = _controllers.FirstOrDefault(c => c.Name == controllerName);
        if (controller == null)
            return null;

        var applianceRecord = new ApplianceModel
        {
            ControllerName = controller.Name,
            Configuration = "" // No configuration for now
        };

        // We need to add the appliance to the database to get an id
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplianceContext>();
        context.Appliances.Add(applianceRecord);
        await context.SaveChangesAsync();

        // Now we can add the appliance to the controller
        var appliance = await controller.AddApplianceAsync(applianceRecord.Id);
        
        // Update the configuration
        applianceRecord.Configuration = appliance?.Configuration ?? "";
        await context.SaveChangesAsync();

        if (appliance != null)
            _appliances.Add(appliance);

        return appliance;
    }

    /// <summary>
    ///   Removes an appliance from the database and the controller
    /// </summary>
    /// <param name="appliance"> The appliance to remove </param>
    /// <returns> True if the appliance was removed successfully, false otherwise </returns>
    public async Task<bool> RemoveApplianceAsync(IAppliance appliance)
    {
        // try to delete the appliance from the controller
        var result = await appliance.Controller.DeleteApplianceAsync(appliance.Id);

        if (!result)
            return false;

        _appliances.Remove(appliance);

        // remove the appliance record from the database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplianceContext>();
        var applianceRecord = context.Appliances.Find(appliance.Id);
        if (applianceRecord != null)
        {
            context.Appliances.Remove(applianceRecord);
            await context.SaveChangesAsync();
        }
        return true;
    }

    /// <summary>
    ///  Updates the configuration of an appliance in the database
    /// </summary>
    /// <param name="appliance"> The updated appliance </param>
    /// <returns> True if the appliance was updated successfully, false otherwise </returns>
    public async Task<bool> UpdateApplianceAsync(IAppliance appliance)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplianceContext>();
        var applianceRecord = context.Appliances.Find(appliance.Id);
        if (applianceRecord == null)
            return false;

        applianceRecord.Configuration = appliance.Configuration;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<string[]> GetAvailableControllersAsync()
    {
        return await Task.FromResult(_controllers.Select(c => c.Name).ToArray());
    }
}