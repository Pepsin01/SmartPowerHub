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
public record ProductionPlan(TimeSlot[] TimeSlots, int TimeSlotLength, DateTime StartTime)
{
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

public class PlanningService(IServiceProvider serviceProvider)
{
    private readonly PlanPredictor _planPredictor = new(serviceProvider);

    public Task<ProductionPlan> GetPlan()
    {
        return Task.FromResult(_planPredictor.GenerateNormalSolarPlan(DateTime.Now, 48, 60));
    }
}