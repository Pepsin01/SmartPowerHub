using IoTControllerContracts;

namespace SmartPowerHub.Data;

/// <summary>
/// Class that generates production plans for various energy sources and based on various data.
/// </summary>
/// <param name="serviceProvider"> The service provider to use for dependency injection </param>
public class PlanPredictor(IServiceProvider serviceProvider)
{
    /// <summary>
    /// Generates a production plan for a normal solar day and currently available solar sources.
    /// </summary>
    /// <param name="startTime"> The start time of the production plan </param>
    /// <param name="timeSlots"> The number of time slots to generate </param>
    /// <param name="timeSlotLength"> The length of each time slot in minutes </param>
    /// <returns> A production plan for a normal solar day </returns>
    public ProductionPlan GenerateNormalSolarPlan(DateTime startTime, int timeSlots, int timeSlotLength)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DeviceService<IEnergySource>>();

        //get max power output of all IEnergySource devices with the type ISolarSource
        var sources = context.GetDevicesAsync().Result.OfType<ISolarSource>();
        var maxValue = sources.Sum(s => s.GetMaxPowerOutput().Result);

        // preset dawn and dusk times (5:00 and 21:00)
        var dawn = new TimeSpan(5, 0, 0);
        var dusk = new TimeSpan(21, 0, 0);

        var timeSlotsArray =
            GenerateNormalDistributionValues(startTime, dawn, dusk, timeSlots, timeSlotLength, maxValue);
        return new ProductionPlan(timeSlotsArray, timeSlotLength, startTime);
    }

    public ProductionPlan GenerateCloudinessSolarPlan(DateTime startTime, int timeSlots, int timeSlotLength)
    {
        return GenerateNormalSolarPlan(startTime, timeSlots, timeSlotLength);
    }

    /// <summary>
    ///     Generates a normal distribution simulating solar power values
    /// </summary>
    /// <param name="x">The value to calculate the normal distribution for</param>
    /// <returns>The value of the normal distribution at x</returns>
    private double SolarNormalDistribution(double x)
    {
        const double mean = 0.5;
        const double standardDeviation = 0.17;
        var maxDensity = 1 / (standardDeviation * Math.Sqrt(2 * Math.PI));
        var density = 1 / (standardDeviation * Math.Sqrt(2 * Math.PI)) *
                      Math.Exp(-0.5 * Math.Pow((x - mean) / standardDeviation, 2));
        return density / maxDensity;
    }

    /// <summary>
    ///     Generates a set of solar power values using a normal distribution
    /// </summary>
    /// <param name="startTime"> The start time of the time slots </param>
    /// <param name="dawn"> The time of dawn </param>
    /// <param name="dusk"> The time of dusk </param>
    /// <param name="numberOfSlots"> The number of time slots to generate </param>
    /// <param name="timeSlotLength"> The length of each time slot in minutes </param>
    /// <param name="maxValue"> The maximum value of the power capacity </param>
    /// <returns> An array of TimeSlot objects representing the power values </returns>
    public TimeSlot[] GenerateNormalDistributionValues(DateTime startTime, TimeSpan dawn, TimeSpan dusk,
        int numberOfSlots, int timeSlotLength, double maxValue)
    {
        var timeSlots = new TimeSlot[numberOfSlots];
        var currentTime = startTime;

        for (var i = 0; i < numberOfSlots; i++)
        {
            if (currentTime.TimeOfDay >= dawn && currentTime.TimeOfDay <= dusk)
            {
                var hoursFromStart = (currentTime.TimeOfDay - dawn).TotalHours;
                var x = hoursFromStart /
                        (dusk - dawn).TotalHours; // Normalize time between dawn and dusk to range 0 to 1
                var value = SolarNormalDistribution(x);
                timeSlots[i].PowerCapacity = value * maxValue;
            }
            else
            {
                timeSlots[i].PowerCapacity = 0;
            }

            currentTime = currentTime.AddMinutes(timeSlotLength);
        }

        return timeSlots;
    }
}