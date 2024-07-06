using IoTControllerContracts;
using Serilog;
using System.IO;
using System.Reflection;
using System.Text;

namespace SmartPowerHub.Data
{
    public static class ControllerService
    {
        private static readonly string Path = System.IO.Path.Combine(Environment.CurrentDirectory, "IoTControllers");
        public static void UpdateControllers<TDevice>(List<IController> controllers) where TDevice : IDevice
        {
            //Log.Information($"Searching for controllers in {_path}");
            //Log.Information($"Found type: {nameof(T)}");
            var files = Directory.GetFiles(Path, "*.dll");
            foreach (var file in files)
                try
                {
                    //Log.Information($"Found controller dll: {file}");
                    var assembly = Assembly.LoadFrom(file);
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {

                        // check if the found type implements T interface
                        var controller = CreateInstance<TDevice>(type);

                        if (controller == null) continue;
                        
                        // add controller to list if there is no controller with the same name
                        if (!controllers.Exists(c => c.Name == controller.Name))
                            controllers.Add(controller);
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
        }

        private static IController? CreateInstance<TDevice>(Type type) where TDevice : IDevice
        {
            Type requiredType;
            if (typeof(TDevice) == typeof(IAppliance))
                requiredType = typeof(IApplianceController);
            else if (typeof(TDevice) == typeof(IBattery))
                requiredType = typeof(IBatteryController);
            else if (typeof(TDevice) == typeof(IEnergySource))
                requiredType = typeof(IEnergySourceController);
            else
                return null;

            if (type.GetInterface(requiredType.Name) == null) return null;

            return Activator.CreateInstance(type) as IController;
        }
    }
}
