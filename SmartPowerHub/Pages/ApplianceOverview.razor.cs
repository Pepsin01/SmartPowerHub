using System.ComponentModel.DataAnnotations;
using IoTControllerContracts;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace SmartPowerHub.Pages;

public partial class ApplianceOverview
{
    private string[] _availableControllers = [];
    private List<DisplayAppliance>? _displayableAppliances;
    private DisplayAppliance _selectedAppliance;
    private bool IsChooseControllerVisible { get; set; }
    private bool IsLoadingVisible { get; set; }
    private bool IsConfigDialogVisible { get; set; }
    private bool IsDeleteDialogVisible { get; set; }

    private string SelectedController { get; set; } = string.Empty;

    private void ToggleControllerPicker()
    {
        IsChooseControllerVisible = !IsChooseControllerVisible;
    }

    private void ToggleApplianceConfiguration()
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
        _ = LoadAppliancesAsync();
        _ = RefreshAppliancesAsync();
    }

    private async Task GetAvailableControllers()
    {
        var controllers = await Task.Run(() => ApplianceService.GetAvailableControllersAsync());
        Logger.Information("Fetched {Count} controllers", controllers.Length);
        foreach (var controller in controllers) {
            Logger.Information("Controller: {Name}", controller);
        }
        _availableControllers = controllers;
    }

    private async Task LoadAppliancesAsync()
    {
        var appliances = await Task.Run(() => ApplianceService.GetDevicesAsync());
        Logger.Information("Fetched {Count} appliances", appliances.Length);
        _displayableAppliances = appliances.Select(a => new DisplayAppliance(a, this)).ToList();
        StateHasChanged();
    }

    private async Task RefreshAppliancesAsync()
    {
        while (true)
        {
            Task.WaitAll(_displayableAppliances.Select(a => a.Refresh()).ToArray());
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

        AddAppliance();
    }

    /// <summary>
    ///     Tries to add a new appliance to the appliance service.
    /// </summary>
    private void AddAppliance()
    {
        var newAppliance = ApplianceService.AddDeviceAsync(SelectedController).Result;
        IsLoadingVisible = false;
        if (newAppliance == null)
        {
            Snackbar.Add("Failed to add appliance", Severity.Error);
        }
        else
        {
            Snackbar.Add("Appliance added successfully", Severity.Success);
            var newDisplayAppliance = new DisplayAppliance(newAppliance, this);
            _displayableAppliances?.Add(newDisplayAppliance);
            newDisplayAppliance.ToggleSelected();
            IsConfigDialogVisible = true;
        }
    }

    /// <summary>
    ///     Class to hold the displayable appliance data for the overview page.
    /// </summary>
    /// <param name="appliance"> The appliance to display. </param>
    /// <param name="ao"> The appliance overview page. </param>
    private class DisplayAppliance
    {
        private readonly ApplianceOverview _ao;

        public DisplayAppliance(IAppliance appliance, ApplianceOverview ao)
        {
            Appliance = appliance;
            _ao = ao;
            Validator = new EditApplianceValidator(this);
            _ = Refresh();
        }

        public IAppliance Appliance { get; }
        public bool IsSelected { get; private set; }
        public bool IsOnline { get; private set; }
        public EditApplianceValidator Validator { get; }

        public async Task Refresh()
        {
            IsOnline = await Appliance.IsOnlineAsync();
        }

        public void ToggleSelected()
        {
            foreach (var appliance in _ao._displayableAppliances)
                if (appliance != this)
                    appliance.IsSelected = false;
            IsSelected = !IsSelected;
            _ao._selectedAppliance = this;
        }

        public void OnValidEditSubmit(EditContext context)
        {
            Appliance.Name = _ao._selectedAppliance.Validator.Name;
            Appliance.Description = _ao._selectedAppliance.Validator.Description;
            _ao.IsConfigDialogVisible = false;
            _ao.IsLoadingVisible = true;
            var result = _ao.ApplianceService.UpdateDeviceAsync(Appliance).Result;
            _ao.IsLoadingVisible = false;
            if (result)
                _ao.Snackbar.Add("Appliance updated successfully", Severity.Success);
            else
                _ao.Snackbar.Add("Failed to update appliance", Severity.Error);

            IsSelected = false;
            _ao.StateHasChanged();
        }

        public void ShowConsole()
        {
            var parameters = new DialogParameters<ConsoleDialog>
            {
                { x => x.Device, Appliance }
            };
            var options = new DialogOptions { MaxWidth = MaxWidth.Large, FullWidth = true, CloseButton = true };
            _ao.DialogService.Show<ConsoleDialog>($"Console for {Appliance.Name}", parameters, options);
        }

        public void DeleteAppliance()
        {
            _ao.IsLoadingVisible = true;
            var result = _ao.ApplianceService.DeleteDeviceAsync(Appliance).Result;
            _ao.IsLoadingVisible = false;
            if (result)
            {
                _ao.Snackbar.Add("Appliance deleted successfully", Severity.Success);
                _ao._displayableAppliances.Remove(this);
            }
            else
            {
                _ao.Snackbar.Add("Failed to delete appliance", Severity.Error);
            }

            _ao.ToggleDeleteDialog();
            _ = _ao.LoadAppliancesAsync();
        }
    }

    /// <summary>
    ///     Class to hold the form data for editing an appliance.
    /// </summary>
    /// <param name="displayAppliance"> The appliance to edit. </param>
    private class EditApplianceValidator(DisplayAppliance displayAppliance)
    {
        [Required]
        [StringLength(30, ErrorMessage = "Name is too long.")]
        public string Name { get; set; } = displayAppliance.Appliance.Name;

        [Required]
        [StringLength(200, ErrorMessage = "Description is too long.")]
        public string Description { get; set; } = displayAppliance.Appliance.Description;
    }
}