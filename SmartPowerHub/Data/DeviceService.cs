﻿using System.Data.SqlTypes;
using System.Reflection;
using System.Text;
using IoTControllerContracts;
using Serilog;
using SmartPowerHub.Database.Contexts;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Data;

public class DeviceService<TDevice> where TDevice : class, IDevice
{
    private readonly List<IController> _controllers;
    private readonly List<TDevice> _devices;
    private readonly IServiceProvider _serviceProvider;

    public DeviceService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        //_controllers = InitializeAvailableControllers(Path.Combine(Environment.CurrentDirectory, "IoTControllers"));
        _controllers = new List<IController>();
        ControllerService.UpdateControllers<TDevice>(_controllers);
        _devices = InitializeDevices();
    }

    /// <summary>
    ///    Initializes all devices from the database
    /// </summary>
    /// <returns> A list of devices initialized from the database </returns>
    private List<TDevice> InitializeDevices()
    {
        Log.Information("Initializing devices from database");

        // Get all device records from the database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        var deviceRecords = context.Devices.ToList();
        var devices = new List<TDevice>();

        // Add all devices to the controllers
        foreach (var record in deviceRecords)
        {
            var device = InitializeFromRecord(record);
            if (device != null)
                devices.Add(device);
        }

        return devices;
    }

    private void UpdateControllers()
    {
        ControllerService.UpdateControllers<TDevice>(_controllers);
    }

    private void TryInitializeNewDevices()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        var deviceRecords = context.Devices.ToList();

        foreach (var record in deviceRecords)
        {
            if (_devices.Any(d => d.Id == record.Id))
                continue;

            var device = InitializeFromRecord(record);
            if (device != null)
                _devices.Add(device);
        }
    }
    private TDevice? InitializeFromRecord(DeviceModel record)
    {
        var controller = _controllers.FirstOrDefault(c => c.Name == record.ControllerName);
        if (controller == null)
            return null;

        return controller switch
        {
            IApplianceController applianceController => applianceController
                .AddDeviceWithConfigAsync(record.Id, record.Configuration)
                .Result as TDevice,
            IBatteryController batteryController => batteryController
                .AddDeviceWithConfigAsync(record.Id, record.Configuration)
                .Result as TDevice,
            IEnergySourceController energySourceController => energySourceController
                .AddDeviceWithConfigAsync(record.Id, record.Configuration)
                .Result as TDevice,
            _ => null
        };
    }

    /// <summary>
    ///   Gets all devices of the specified type
    /// </summary>
    /// <returns> An array of devices of the specified type </returns>
    public Task<TDevice[]> GetDevicesAsync()
    {
        UpdateControllers();
        TryInitializeNewDevices();
        return Task.FromResult(_devices.ToArray());
    }

    /// <summary>
    ///  Adds a new device to the database and the controller
    /// </summary>
    /// <param name="controllerName"> The name of the controller to add the device to </param>
    /// <returns> The added device if successful, null otherwise </returns>
    public async Task<TDevice?> AddDeviceAsync(string controllerName)
    {
        var controller = _controllers.FirstOrDefault(c => c.Name == controllerName);
        if (controller == null)
            return null;

        var deviceRecord = new DeviceModel
        {
            ControllerName = controller.Name,
            Configuration = "" // No configuration for now
        };

        // We need to add the device to the database to get an id
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        context.Devices.Add(deviceRecord);
        await context.SaveChangesAsync();

        // Now we can add the device to the controller
        var device = await AddDeviceForController(controller, deviceRecord.Id);

        if (device != null)
        {
            // Update the configuration
            deviceRecord.Configuration = device.Configuration ?? "";
            await context.SaveChangesAsync();
            _devices.Add(device);
        }
        else
        {
            // If the device could not be added to the controller, remove the record from the database
            context.Devices.Remove(deviceRecord);
            await context.SaveChangesAsync();
        }

        return device;
    }

    private async Task<TDevice?> AddDeviceForController(IController controller, int id)
    {
        var device = controller switch
        {
            IApplianceController applianceController => await applianceController
                .AddDeviceAsync(id) as TDevice,
            IBatteryController batteryController => await batteryController
                .AddDeviceAsync(id) as TDevice,
            IEnergySourceController energySourceController => await energySourceController
                .AddDeviceAsync(id) as TDevice,
            _ => null
        };

        return device;
    }

    /// <summary>
    /// Removes a device from the database and the controller
    /// </summary>
    /// <param name="device"> The device to remove </param>
    /// <returns> True if the device was removed successfully, false otherwise </returns>
    public async Task<bool> DeleteDeviceAsync(TDevice device)
    {
        // try to delete the device from the controller
        var result = await device.Controller.DeleteDeviceAsync(device.Id);

        if (!result)
            return false;

        _devices.Remove(device);

        // remove the device record from the database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        var deviceRecord = context.Devices.Find(device.Id);
        if (deviceRecord != null)
        {
            context.Devices.Remove(deviceRecord);
            await context.SaveChangesAsync();
        }
        return true;
    }

    /// <summary>
    ///  Updates the configuration of a device in the database
    /// </summary>
    /// <param name="device"> The updated device </param>
    /// <returns> True if the device was updated successfully, false otherwise </returns>
    public async Task<bool> UpdateDeviceAsync(IDevice device)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        var deviceRecord = context.Devices.Find(device.Id);
        if (deviceRecord == null)
            return false;

        deviceRecord.Configuration = device.Configuration;
        await context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Gets names of all available controllers of the specified type
    /// </summary>
    /// <returns> An array of names of the available controllers </returns>
    public async Task<string[]> GetAvailableControllersAsync()
    {
        return await Task.FromResult(_controllers.Select(c => c.Name).ToArray());
    }
}