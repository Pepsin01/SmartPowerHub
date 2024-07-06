using System.ComponentModel.DataAnnotations;
using IoTControllerContracts;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using SmartPowerHub.Data;

namespace SmartPowerHub.Pages;

public partial class EnergySourceManagement
{
    private string[] _availableBatteryControllers = [];
    private string[] _availableEnergySourceControllers = [];
    private List<DisplayDevice> _displayableBatteries = [];
    private List<DisplayDevice> _displayableEnergySources = [];
    private DisplayDevice _selectedDevice;
    private bool IsChooseControllerVisible { get; set; }
    private bool BatteryControllerPicker { get; set; }
    private bool IsLoadingVisible { get; set; }
    private bool IsConfigDialogVisible { get; set; }
    private bool IsDeleteDialogVisible { get; set; }
    private string SelectedController { get; set; } = string.Empty;

    private void ToggleEnergySourceControllerPicker()
    {
        BatteryControllerPicker = false;
        ToggleControllerPicker();
    }

    private void ToggleBatteryControllerPicker()
    {
        BatteryControllerPicker = true;
        ToggleControllerPicker();
    }

    private void ToggleControllerPicker()
    {
        IsChooseControllerVisible = !IsChooseControllerVisible;
    }

    private void ToggleDeviceConfiguration()
    {
        IsConfigDialogVisible = !IsConfigDialogVisible;
    }

    private void ToggleDeleteDialog()
    {
        IsDeleteDialogVisible = !IsDeleteDialogVisible;
    }

    protected override void OnInitialized()
    {
        Logger.Information("ApplianceOverview page initialized");
        _ = GetAvailableControllers();
        _ = LoadDevicesAsync();
        _ = RefreshDevicesAsync();
    }

    private async Task GetAvailableControllers()
    {
        var batteryControllers = await Task.Run(() => BatteryService.GetAvailableControllersAsync());
        Logger.Information("Fetched {Count} battery controllers", batteryControllers.Length);
        _availableBatteryControllers = batteryControllers;

        var energySourceControllers = await Task.Run(() => EnergySourceService.GetAvailableControllersAsync());
        Logger.Information("Fetched {Count} energy source controllers", energySourceControllers.Length);
        _availableEnergySourceControllers = energySourceControllers;
    }

    private async Task LoadDevicesAsync()
    {
        var batteries = await Task.Run(() => BatteryService.GetDevicesAsync());
        Logger.Information("Fetched {Count} batteries", batteries.Length);
        _displayableBatteries = batteries.Select(a => new DisplayDevice(a, this)).ToList();

        var energySources = await Task.Run(() => EnergySourceService.GetDevicesAsync());
        Logger.Information("Fetched {Count} energy sources", energySources.Length);
        _displayableEnergySources = energySources.Select(a => new DisplayDevice(a, this)).ToList();

        StateHasChanged();
    }

    private async Task RefreshDevicesAsync()
    {
        while (true)
        {
            Task.WaitAll(_displayableEnergySources.Select(a => a.Refresh()).ToArray());
            Task.WaitAll(_displayableBatteries.Select(a => a.Refresh()).ToArray());
            StateHasChanged();
            await Task.Delay(5000);
        }
    }

    private void SubmitSelectedController()
    {
        IsChooseControllerVisible = false;
        IsLoadingVisible = true;
        if (SelectedController == string.Empty)
        {
            IsLoadingVisible = false;
            return;
        }

        AddDevice(BatteryControllerPicker);
    }

    /// <summary>
    ///     Tries to add a new appliance to the appliance service.
    /// </summary>
    private void AddDevice(bool isBattery)
    {
        IDevice? newDevice = isBattery
            ? BatteryService.AddDeviceAsync(SelectedController).Result
            : EnergySourceService.AddDeviceAsync(SelectedController).Result;

        IsLoadingVisible = false;
        SelectedController = string.Empty;

        if (newDevice == null)
        {
            Snackbar.Add("Failed to add device", Severity.Error);
        }
        else
        {
            Snackbar.Add("Device added successfully", Severity.Success);
            var newDisplayDevice = new DisplayDevice(newDevice, this);
            if (isBattery)
                _displayableBatteries.Add(newDisplayDevice);
            else
                _displayableEnergySources.Add(newDisplayDevice);
            newDisplayDevice.ToggleSelected();
            IsConfigDialogVisible = true;
        }
    }

    /// <summary>
    ///     Class to hold the displayable appliance data for the overview page.
    /// </summary>
    /// <param name="appliance"> The appliance to display. </param>
    /// <param name="ao"> The appliance overview page. </param>
    private class DisplayDevice
    {
        private readonly EnergySourceManagement _esm;

        public DisplayDevice(IDevice? device, EnergySourceManagement esm)
        {
            Device = device;
            _esm = esm;
            Validator = new EditApplianceValidator(this);
            _ = Refresh();
        }

        public IDevice? Device { get; }
        public bool IsSelected { get; private set; }
        public bool IsOnline { get; private set; }
        public EditApplianceValidator Validator { get; }

        public async Task Refresh()
        {
            IsOnline = await Device.IsOnlineAsync();
        }

        public void ToggleSelected()
        {
            foreach (var device in _esm._displayableEnergySources)
                if (device != this)
                    device.IsSelected = false;

            foreach (var device in _esm._displayableBatteries)
                if (device != this)
                    device.IsSelected = false;

            IsSelected = !IsSelected;
            _esm._selectedDevice = this;
        }

        public void OnValidEditSubmit(EditContext context)
        {
            Device.Name = _esm._selectedDevice.Validator.Name;
            Device.Description = _esm._selectedDevice.Validator.Description;
            _esm.IsConfigDialogVisible = false;
            _esm.IsLoadingVisible = true;

            var result = Device is IBattery
                ? _esm.BatteryService.UpdateDeviceAsync(Device).Result
                : _esm.EnergySourceService.UpdateDeviceAsync(Device).Result;

            _esm.IsLoadingVisible = false;
            if (result)
                _esm.Snackbar.Add("Device updated successfully", Severity.Success);
            else
                _esm.Snackbar.Add("Failed to update appliance", Severity.Error);

            IsSelected = false;
            _esm.StateHasChanged();
        }

        public void ShowConsole()
        {
            var parameters = new DialogParameters<ConsoleDialog>
            {
                { x => x.Device, Device }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseButton = true };
            _esm.DialogService.Show<ConsoleDialog>($"Console for {Device.Name}", parameters, options);
        }

        public void DeleteDevice()
        {
            _esm.IsLoadingVisible = true;
            var result = Device is IBattery
                ? _esm.BatteryService.UpdateDeviceAsync(Device).Result
                : _esm.EnergySourceService.UpdateDeviceAsync(Device).Result;
            _esm.IsLoadingVisible = false;
            if (result)
            {
                _esm.Snackbar.Add("Device deleted successfully", Severity.Success);
            }
            else
            {
                _esm.Snackbar.Add("Failed to delete appliance", Severity.Error);
            }

            _esm.ToggleDeleteDialog();
            _ = _esm.LoadDevicesAsync();
        }
    }

    /// <summary>
    ///     Class to hold the form data for editing an appliance.
    /// </summary>
    /// <param name="displayDevice"> The appliance to edit. </param>
    private class EditApplianceValidator(DisplayDevice displayDevice)
    {
        [Required]
        [StringLength(30, ErrorMessage = "Name is too long.")]
        public string Name { get; set; } = displayDevice.Device.Name;

        [Required]
        [StringLength(200, ErrorMessage = "Description is too long.")]
        public string Description { get; set; } = displayDevice.Device.Description;
    }
}