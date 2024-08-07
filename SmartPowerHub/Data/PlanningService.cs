﻿using IoTControllerContracts;
using Serilog;
using SmartPowerHub.Database.Contexts;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Data;

/// <summary>
///     Represents a time slot in a production plan.
/// </summary>
public struct TimeSlot
{
    /// <summary>
    ///     Power production in Wh.
    /// </summary>
    public double PowerCapacity;
}

/// <summary>
///     Represents a production plan of a planned production timespan.
/// </summary>
/// <param name="TimeSlots"> Predicted power production for each time slot </param>
/// <param name="TimeSlotLength"> Length of each TimeSlot in minutes </param>
/// <param name="StartTime"> Start time of the production plan </param>
public record ProductionPlan(string Name, TimeSlot[] TimeSlots, int TimeSlotLength, DateTime StartTime)
{
    public readonly string Name = Name;

    /// <summary>
    ///     Start time of the production plan.
    /// </summary>
    public readonly DateTime StartTime = StartTime;

    /// <summary>
    ///     Length of each TimeSlot in minutes.
    /// </summary>
    public readonly int TimeSlotLength = TimeSlotLength;

    /// <summary>
    ///     Predicted power production for each time slot.
    /// </summary>
    public readonly TimeSlot[] TimeSlots = TimeSlots;
}

/// <summary>
/// represents a scheduled program
/// </summary>
public record ScheduledProgram
{
    public int Id { get; init; }
    public IProgram Program { get; init; }
    public DateTime StartTime { get; init; }
}

/// <summary>
///    Service for planning appliance programs.
/// </summary>
public class PlanningService
{
    private readonly PlanPredictor _planPredictor;
    private readonly IServiceProvider _serviceProvider;
    private ProductionPlan? _currentPlan;

    public PlanningService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _planPredictor = new PlanPredictor(serviceProvider);
        RemoveAllSchedule();
        Task.Run(ScheduledProgramsRunner);
    }

    private void RemoveAllSchedule()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        context.ScheduledPrograms.RemoveRange(context.ScheduledPrograms);
        context.SaveChanges();
    }

    /// <summary>
    /// Method to get the current production plan
    /// </summary>
    /// <returns> The current production plan </returns>
    public Task<ProductionPlan?> GetCurrentProductionPlan()
    {
        return Task.FromResult(_currentPlan);
    }

    /// <summary>
    ///    Plans appliance programs for normal solar production plan.
    /// </summary>
    /// <param name="programs"> Programs to plan </param>
    /// <param name="startTime"> Start time of the production plan </param>
    /// <param name="timeSlotLength"> Length of each TimeSlot in minutes </param>
    /// <param name="timeSlotCount"> Number of time slots to plan </param>
    /// <returns></returns>
    public Task<ProductionPlan> PlanProgramsNormalSolar(IProgram[] programs, DateTime startTime, int timeSlotLength,
        int timeSlotCount)
    {
        var plan = _planPredictor.GenerateNormalSolarPlan(startTime, timeSlotLength, timeSlotCount);

        if (programs.Length > 0)
        {
            var scheduledPrograms = Planner.SchedulePrograms(programs, plan);

            Log.Information("Planned {ProgramCount} programs for normal solar production", scheduledPrograms.Length);

            SaveScheduledPrograms(scheduledPrograms);
        }

        return Task.FromResult(plan);
    }

    private void SaveScheduledPrograms(ScheduledProgramModel[] scheduledPrograms)
    {
        // Save scheduled programs to database
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        context.ScheduledPrograms.AddRange(scheduledPrograms);
        context.SaveChanges();
        Log.Information("Saved {ProgramCount} scheduled programs to database", scheduledPrograms.Length);
    }

    /// <summary>
    /// Method to get the scheduled programs
    /// </summary>
    /// <returns> The scheduled programs </returns>
    public Task<ScheduledProgram[]> GetScheduledPrograms()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        var scheduledPrograms = context.ScheduledPrograms.ToList();
        return Task.FromResult(scheduledPrograms.Select(ConvertFromModel).Where(p => p != null).Select(p => p!)
            .ToArray());
    }

    private ScheduledProgram? ConvertFromModel(ScheduledProgramModel model)
    {
        using var scope = _serviceProvider.CreateScope();
        var deviceService = scope.ServiceProvider.GetRequiredService<DeviceService<IAppliance>>();
        var appliance = deviceService.GetDevicesAsync().Result.FirstOrDefault(d => d.Id == model.DeviceId);
        if (appliance == null)
            return null;

        var program = appliance.GetProgramsAsync().Result.FirstOrDefault(p => p.Name == model.ProgramName);

        return program == null
            ? null
            : new ScheduledProgram { Id = model.Id, Program = program, StartTime = model.StartTime };
    }

    /// <summary>
    /// Method to remove a scheduled program
    /// </summary>
    /// <param name="id"> The id of the scheduled program to remove </param>
    /// <returns> True if the scheduled program was removed, false otherwise </returns>
    public Task<bool> RemoveScheduledProgram(int id)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();
        var scheduledProgram = context.ScheduledPrograms.FirstOrDefault(p => p.Id == id);
        if (scheduledProgram == null)
            return Task.FromResult(false);

        context.ScheduledPrograms.Remove(scheduledProgram);
        Log.Information("Removed scheduled program {ProgramName} for device {DeviceId}", scheduledProgram.ProgramName,
            scheduledProgram.DeviceId);
        context.SaveChanges();
        return Task.FromResult(true);
    }

    private void ScheduledProgramsRunner()
    {
        // Run scheduled programs
        while (true)
        {
            var scheduledPrograms = GetScheduledPrograms().Result;
            foreach (var scheduledProgram in scheduledPrograms)
                if (DateTime.Now >= scheduledProgram.StartTime)
                {
                    var result = scheduledProgram.Program.StartAsync().Result;
                    if (result)
                        RemoveScheduledProgram(scheduledProgram.Id);
                }

            Thread.Sleep(10000);
        }
    }
}