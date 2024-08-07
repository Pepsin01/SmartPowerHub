﻿using IoTControllerContracts;
using Microsoft.AspNetCore.Components.Forms;
using Serilog;
using System.IO;
using System.Reflection;
using System.Text;

namespace SmartPowerHub.Data
{
    /// <summary>
    /// Service for managing controllers.
    /// </summary>
    /// <param name="serviceProvider"> Service provider for creating instances </param>
    public class ControllerService(IServiceProvider serviceProvider)
    {
        private static readonly string Path = System.IO.Path.Combine(Environment.CurrentDirectory, "IoTControllers");

        /// <summary>
        /// Receives a list of controllers and updates it with the controllers found in the controller directory.
        /// </summary>
        /// <typeparam name="TDevice"> Type of the device the controllers are for </typeparam>
        /// <param name="controllers"> List of controllers to update </param>
        public static void UpdateControllers<TDevice>(List<IController> controllers) where TDevice : IDevice
        {
            //Log.Information($"Searching for controllers in {_path}");
            //Log.Information($"Found type: {nameof(T)}");
            // Create the directory if it doesn't exist
            if (!Directory.Exists(Path))
            {
                Directory.CreateDirectory(Path);
            }
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
                        {
                            controllers.Add(controller);
                            Log.Information($"Added controller {controller.Name} to list.");
                        }
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
                catch (BadImageFormatException ex)
                {
                    Log.Error(ex, $"Error while loading controllers from {file}");
                }
        }

        public async Task<bool> UploadControllers(IReadOnlyList<IBrowserFile> files)
        {
            foreach (var file in files)
            {
                var filePath = System.IO.Path.Combine(Path, file.Name);
                await using var fileStream = new FileStream(filePath, FileMode.Create);
                var readStream = file.OpenReadStream();
                if (readStream.Length == 0)
                {
                    Log.Error($"File {file.Name} is empty.");
                    continue;
                }
                await readStream.CopyToAsync(fileStream);
                Log.Information($"Uploaded {file.Name} successfully.");
            }

            return true;
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
