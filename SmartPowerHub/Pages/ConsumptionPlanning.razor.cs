using System.ComponentModel.DataAnnotations;
using IoTControllerContracts;
using MudBlazor;
using Serilog;
using SmartPowerHub.Data;

namespace SmartPowerHub.Pages;

public partial class ConsumptionPlanning
{
    private readonly List<DisplayableProgram> _displayedPrograms = [];
    private readonly List<ChartSeries> _chartData = [];
    private readonly string _plannerZone = "PlannerZone";
    private readonly PlanSetupForm _planSetupModel = new();
    private readonly string _readyZone = "ReadyZone";
    private DisplayableAppliance[]? _appliances;
    private MudDropContainer<DisplayableProgram> _dropContainer;
    private string[]? _chartLabels;
    private ScheduledProgram[] _scheduledPrograms { get; set; } = [];
    private bool IsPlanSetUpDialogVisible { get; set; }
    private bool IsLoadingVisible { get; set; }

    private void TogglePlanSetUpDialog()
    {
        IsPlanSetUpDialogVisible = !IsPlanSetUpDialogVisible;
    }

    protected override void OnInitialized()
    {
        _ = LoadData();
        _ = PeriodicUpdate();
    }

    private async Task LoadData()
    {
        var appliances = await Task.Run(() => ApplianceService.GetDevicesAsync());
        _appliances = appliances.Select(a => new DisplayableAppliance(a, this)).ToArray();
        Task.WaitAll(_appliances.Select(a => a.LoadPrograms()).ToArray());

        var plan = await planningService.GetCurrentProductionPlan();
        if (plan != null)
            UpdateChart(plan);

        StateHasChanged();
    }

    private async Task PeriodicUpdate()
    {
        while (true)
        {
            await UpdateScheduledPrograms();
            await Task.Delay(1000);
        }
    }

    private void UpdateChart(ProductionPlan plan)
    {
        _chartData.Clear();
        var series = new ChartSeries
        {
            Name = plan.Name,
            Data = plan.TimeSlots.Select(t => t.PowerCapacity).ToArray()
        };
        _chartData.Add(series);

        _chartLabels = new string[plan.TimeSlots.Length];

        for (var i = 0; i < plan.TimeSlots.Length; i++)
            _chartLabels[i] = (plan.StartTime + TimeSpan.FromMinutes(i * plan.TimeSlotLength)).ToString("HH");
    }

    private async Task UpdateScheduledPrograms()
    {
        _scheduledPrograms = await planningService.GetScheduledPrograms();
        foreach (var item in _displayedPrograms) await item.Update();

        StateHasChanged();
    }

    private void RemoveAllFromPlanning()
    {
        foreach (var item in _displayedPrograms)
            if (!item.IsDisabled)
            {
                item.Selector = item.DP.DropZoneId;
                item.DP.RefreshDragDropPrograms();
            }

        _dropContainer.Refresh();
    }

    private void UnschedulePrograms()
    {
        foreach (var item in _displayedPrograms.Where(item => item.Selector == _readyZone)) _ = item.Unschedule();
    }

    private void SendPlanSetup()
    {
        IsPlanSetUpDialogVisible = false;
        IsLoadingVisible = true;

        Log.Information("PlanType: {PlanType}, StartTime: {StartTime}, PlanDuration: {PlanDuration}",
            _planSetupModel.PlanType, _planSetupModel.StartTime, _planSetupModel.PlanDuration);

        var programs = GetSelectedPrograms();


        ProductionPlan plan = null;

        if (_planSetupModel.PlanType == "Normal Solar Plan")
            plan = planningService
                .PlanProgramsNormalSolar(programs, _planSetupModel.StartTime, 15, _planSetupModel.PlanDuration * 4)
                .Result;

        UpdateChart(plan);

        IsLoadingVisible = false;
    }

    private IProgram[] GetSelectedPrograms()
    {
        return _displayedPrograms.Where(i => i.Selector == _plannerZone).Select(i => i.Program).ToArray();
    }

    private void ItemUpdated(MudItemDropInfo<DisplayableProgram> dropItem)
    {
        dropItem.Item.Selector = dropItem.DropzoneIdentifier;
        dropItem.Item.DP.RefreshDragDropPrograms();
    }

    private class DisplayableAppliance
    {
        public readonly ConsumptionPlanning Co;

        public DisplayableAppliance(IAppliance appliance, ConsumptionPlanning co)
        {
            Co = co;
            Appliance = appliance;
        }

        public IAppliance Appliance { get; }
        public DisplayableProgram[] Programs { get; set; }
        public string DropZoneId => "Drop zone: " + Appliance.Id;

        public async Task LoadPrograms()
        {
            var programs = await Appliance.GetProgramsAsync();
            Programs = programs.Select(p => new DisplayableProgram(p, DropZoneId, this)).ToArray();
            foreach (var displayableProgram in Programs) await displayableProgram.LoadState();

            Co._displayedPrograms.AddRange(Programs);
        }

        public void RefreshDragDropPrograms()
        {
            var hasPlannedProgram = false;
            var hasScheduledProgram = false;
            foreach (var program in Programs)
            {
                if (program.Selector == Co._plannerZone)
                {
                    hasPlannedProgram = true;
                    break;
                }
                if (program.Selector == Co._readyZone)
                {
                    hasScheduledProgram = true;
                    break;
                }
            }

            if (hasPlannedProgram)
            {
                foreach (var program in Programs) program.IsDisabled = program.Selector == DropZoneId;
                return;
            }
            if (hasScheduledProgram)
            {
                foreach (var program in Programs) program.IsDisabled = true;
                return;
            }
            foreach (var program in Programs) program.IsDisabled = program.Status is ProgramStatus.Running or ProgramStatus.Unavailable;
        }
    }

    private class DisplayableProgram
    {
        private DateTime? _plannedStartTime;

        public DisplayableProgram(IProgram program, string selector, DisplayableAppliance dp)
        {
            Program = program;
            Selector = selector;
            DP = dp;
            _plannedStartTime = null;
        }

        public IProgram Program { get; }
        public ProgramStatus Status { get; private set; }
        public string Selector { get; set; }
        public DisplayableAppliance DP { get; }
        public bool IsDisabled { get; set; }

        private void SetPlannedStartTime(DateTime? plannedStartTime)
        {
            _plannedStartTime = plannedStartTime;
        }

        public async Task Update()
        {
            await LoadState();
            if (Status == ProgramStatus.Available)
            {
                foreach (var scheduledProgram in DP.Co._scheduledPrograms)
                {
                    if (scheduledProgram.Program == Program)
                    {
                        SetPlannedStartTime(scheduledProgram.StartTime);
                        Selector = DP.Co._readyZone;
                        Log.Information("Program {ProgramName} is scheduled at {StartTime}", Program.Name,
                            scheduledProgram.StartTime);
                    }
                    else
                    {
                        SetPlannedStartTime(null);
                    }
                }
            }
            else if (Status == ProgramStatus.Unavailable)
            {
                SetPlannedStartTime(null);
                Selector = DP.DropZoneId;
            }
            else if (Status == ProgramStatus.Running)
            {
                SetPlannedStartTime(null);
                Selector = DP.DropZoneId;
            }

            DP.RefreshDragDropPrograms();
            DP.Co._dropContainer.Refresh();
        }

        public string GetPlannedStartTime()
        {
            return _plannedStartTime?.ToString("HH:mm") ?? "Not scheduled";
        }

        public async Task LoadState()
        {
            var status = await Program.GetStatusAsync();
            Status = status;
        }

        public Color GetColor()
        {
            return Status switch
            {
                ProgramStatus.Available => Color.Success,
                ProgramStatus.Unavailable => Color.Error,
                ProgramStatus.Running => Color.Warning,
                _ => Color.Primary
            };
        }

        public string ButtonText()
        {
            if (Selector == DP.Co._readyZone)
                return "Disabled";

            return Selector == DP.Co._plannerZone ? "Remove" : "Add to planning";
        }

        public Color ButtonColor()
        {
            if (Selector == DP.Co._plannerZone || Selector == DP.Co._plannerZone)
                return Color.Error;

            return Color.Secondary;
        }

        public async Task Unschedule()
        {
            foreach (var scheduledProgram in DP.Co._scheduledPrograms)
                if (scheduledProgram.Program == Program)
                {
                    await DP.Co.planningService.RemoveScheduledProgram(scheduledProgram.Id);
                    break;
                }

            Update().Wait();
        }

        public void SwitchDropZone()
        {
            if (IsDisabled)
                return;
            if (Selector == DP.Co._plannerZone)
                Selector = DP.DropZoneId;
            else
                Selector = DP.Co._plannerZone;

            DP.RefreshDragDropPrograms();
            DP.Co._dropContainer.Refresh();
        }
    }

    private class PlanSetupForm
    {
        [Required(ErrorMessage = "PlanType is required")]
        public string PlanType { get; set; }

        [Required(ErrorMessage = "StartTime is required")]
        [DateTimeRange(ErrorMessage = "StartTime must be within 48 hours from now")]
        public DateTime StartTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "PlanDuration is required")]
        [Range(1, 48, ErrorMessage = "PlanDuration must be between 1 and 48 hours")]
        [DurationWithinRange("StartTime", ErrorMessage = "Start time + duration must be within 48 hours from now!")]
        public int PlanDuration { get; set; } = 24;

        public string OpenWeatherToken { get; set; }
    }

    private class DateTimeRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateTimeValue)
            {
                var now = DateTime.Now;
                if (dateTimeValue >= now && dateTimeValue <= now.AddHours(48)) return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }

    private class DurationWithinRangeAttribute : ValidationAttribute
    {
        private readonly string _startTimePropertyName;

        public DurationWithinRangeAttribute(string startTimePropertyName)
        {
            _startTimePropertyName = startTimePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startTimeProperty = validationContext.ObjectType.GetProperty(_startTimePropertyName);
            if (startTimeProperty == null)
                return new ValidationResult($"Unknown property: {_startTimePropertyName}");

            var startTimeValue = startTimeProperty.GetValue(validationContext.ObjectInstance, null);

            if (value is int duration && startTimeValue is DateTime startTime)
            {
                var now = DateTime.Now;
                if (startTime.AddHours(duration) <= now.AddHours(48))
                    return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}