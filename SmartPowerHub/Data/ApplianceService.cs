using System.Reflection;
using System.Text;
using IoTControllerContracts;
using MudBlazor;

namespace SmartPowerHub.Data
{
    public class ApplianceService
    {
        private readonly IIoTController _controller;
        /**/
        private readonly List<IIoTController> _controllers;
        private List<IAppliance> _appliances = new();

        private List<IIoTController> InitializeAvailableControllers(string path)
        {
            var controllers = new List<IIoTController>();
            var files = Directory.GetFiles(path, "*.dll");
            foreach (var file in files)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.GetInterface(nameof(IIoTController)) == null) continue;
                        if (Activator.CreateInstance(type) is not IIoTController controller) continue;
                        controllers.Add(controller);
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    string errorMessage = sb.ToString();
                    //Display or log the error based on your application.
                    Console.WriteLine(errorMessage);
                }

            }
            return controllers;
        }
        public ApplianceService()
        {
            _controllers = InitializeAvailableControllers(Path.Combine(Environment.CurrentDirectory, "IoTControllers"));
        }

        public Task<List<IAppliance>> GetAppliancesAsync()
        {
            return Task.FromResult(_appliances);
        }
        /**/
    }
}
