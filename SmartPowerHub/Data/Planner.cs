using Google.OrTools.LinearSolver;
using IoTControllerContracts;
using Serilog;
using SmartPowerHub.Database.Models;

namespace SmartPowerHub.Data;

public class Planner
{
    /// <summary>
    ///     Plans the programs to run in the given time slots.
    /// </summary>
    /// <param name="programs"> programs to plan </param>
    /// <param name="productionPlan"> production plan to plan in </param>
    /// <returns> scheduled programs </returns>
    public static ScheduledProgramModel[] SchedulePrograms(IProgram[] programs, ProductionPlan productionPlan)
    {
        var blocksArray = new Block[programs.Length];
        for (var i = 0; i < programs.Length; i++)
            blocksArray[i] = new Block
            {
                Id = programs[i].Appliance.Id,
                // We calculate the power consumption per time slot by dividing the power consumption in watt-hours by the
                // number of time slots the program runs in.
                PowerConsumption = programs[i].PowerConsumptionInWattHours /
                                   ((double)programs[i].RunTimeInMinutes / productionPlan.TimeSlotLength),
                TimeSlotsNeeded = programs[i].RunTimeInMinutes / productionPlan.TimeSlotLength
            };

        // We try to plan the programs using a linear programming solver. If it fails, we fall back to a naive algorithm.
        var plannedBlocks = PlanBlocksLinear(blocksArray, productionPlan.TimeSlots.ToArray()) ??
                            PlanBlocksNaive(blocksArray, productionPlan.TimeSlots.ToArray());

        // Convert the planned programs to scheduled programs
        var scheduledPrograms = new ScheduledProgramModel[plannedBlocks.Length];
        for (var i = 0; i < plannedBlocks.Length; i++)
            scheduledPrograms[i] = new ScheduledProgramModel
            {
                DeviceId = plannedBlocks[i].Id,
                StartTime = productionPlan.StartTime.AddMinutes((double)(plannedBlocks[i].StartTimeSlotIndex *
                                                                         productionPlan.TimeSlotLength)!),
                ProgramName = programs.First(b => b.Appliance.Id == plannedBlocks[i].Id).Name
            };

        return scheduledPrograms;
    }

    /// <summary>
    ///     Plans the blocks to run in the given time slots using a linear programming solver.
    /// </summary>
    /// <param name="blocks"> blocks to plan </param>
    /// <param name="timeSlots"> time slots to plan in </param>
    /// <returns> planned blocks </returns>
    private static Block[]? PlanBlocksLinear(Block[] blocks, TimeSlot[] timeSlots)
    {
        //Solver milp_solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
        var milp_solver = Solver.CreateSolver("SCIP");

        //Variables indicating power overflows in each time slot
        Variable[] tsOverflowVars = milp_solver.MakeNumVarArray(
            timeSlots.Length, 0, double.PositiveInfinity, "TimeSlotOverflows");

        //Offset time slots to prevent negative indexes set to the maximum number of time slots needed by an appliance
        var offset = blocks.Max(b => b.TimeSlotsNeeded);

        //2-D array where each variable means if an appliance starts at a time slot
        var appliancesVars = new Variable[blocks.Length, timeSlots.Length + offset];

        //Create variables for each appliance and time slot
        for (var i = 0; i < blocks.Length; i++)
        for (var j = 0; j < timeSlots.Length + offset; j++)
            appliancesVars[i, j] = milp_solver.MakeBoolVar("Appliance" + i + "TimeSlot" + (j - offset));

        //Constraint -1: First starting time slot of each appliance must be at least the time slot on offset index.
        for (var i = 0; i < blocks.Length; i++)
        for (var j = 0; j < offset; j++)
            milp_solver.Add(appliancesVars[i, j] == 0);


        //Constraint 0: Last starting time slot of each appliance must be at most the last time slot minus the number
        //of time slots the appliance needs
        for (var i = 0; i < blocks.Length; i++)
        for (var j = timeSlots.Length - blocks[i].TimeSlotsNeeded + 1; j < timeSlots.Length; j++)
            milp_solver.Add(appliancesVars[i, j + offset] == 0);

        //Constraint 1: Each appliance must start exactly once
        for (var i = 0; i < blocks.Length; i++)
        {
            var c1 = milp_solver.MakeConstraint(1, 1);
            for (var j = 0; j < timeSlots.Length + offset; j++) c1.SetCoefficient(appliancesVars[i, j], 1);
        }

        //Constraint 2: In each time slot the overflow variable is greater or equal to the sum of power consumptions
        //of appliances that are running in that time slot minus the power capacity of that time slot. And each
        //appliance must run in consecutive time slots for the number of time slots it needs.
        for (var j = offset; j < timeSlots.Length + offset; j++)
        {
            var c2 = milp_solver.MakeConstraint(double.NegativeInfinity, timeSlots[j - offset].PowerCapacity);
            for (var i = 0; i < blocks.Length; i++)
            for (var k = 0; k < blocks[i].TimeSlotsNeeded; k++)
                c2.SetCoefficient(appliancesVars[i, j - k], blocks[i].PowerConsumption);
            c2.SetCoefficient(tsOverflowVars[j - offset], -1);
        }

        //Objective function: Minimize the sum of overflow variables
        var objective = milp_solver.Objective();
        for (var j = 0; j < timeSlots.Length; j++) objective.SetCoefficient(tsOverflowVars[j], 1);
        objective.SetMinimization();

        milp_solver.SetTimeLimit(30000); //30 seconds

        //Solve the problem
        var resultStatus = milp_solver.Solve();

        if (resultStatus != Solver.ResultStatus.OPTIMAL)
        {
            Log.Error("The problem does not have an optimal solution!");
            return null;
        }

        //Return planned blocks
        var plannedBlocks = new Block[blocks.Length];
        for (var i = 0; i < blocks.Length; i++)
        for (var j = offset; j < timeSlots.Length + offset; j++)
            if (Math.Abs(appliancesVars[i, j].SolutionValue() - 1) <= double.NegativeZero)
                plannedBlocks[i] = new Block
                {
                    Id = blocks[i].Id,
                    PowerConsumption = blocks[i].PowerConsumption,
                    TimeSlotsNeeded = blocks[i].TimeSlotsNeeded,
                    StartTimeSlotIndex = j - offset
                };

        return plannedBlocks;
    }

    /// <summary>
    ///     Plans the blocks to run in the given time slots using a naive algorithm.
    /// </summary>
    /// <param name="blocks"> blocks to plan </param>
    /// <param name="timeSlots"> time slots to plan in </param>
    /// <returns> planned blocks </returns>
    private static Block[] PlanBlocksNaive(Block[] blocks, TimeSlot[] timeSlots)
    {
        blocks = blocks.OrderByDescending(b => b.PowerConsumption).ToArray();
        for (var i = 0; i < timeSlots.Length; i++) blocks = RealTimeUpdate(blocks, timeSlots[i], i, timeSlots.Length);
        return blocks;
    }

    /// <summary>
    ///     Is a naive implementation of the real-time update of the blocks.
    ///     Loop through the blocks and start the block if it can be started at the current time slot.
    /// </summary>
    /// <param name="blocks"> blocks to update </param>
    /// <param name="current"> current time slot </param>
    /// <param name="timeSlotIndex"> index of the current time slot </param>
    /// <param name="timeSlotsTotal"> total number of time slots </param>
    /// <returns> updated blocks </returns>
    public static Block[] RealTimeUpdate(Block[] blocks, TimeSlot current, int timeSlotIndex, int timeSlotsTotal)
    {
        // subtract power consumption of running blocks from the current power capacity
        foreach (var block in blocks)
            if (block.StartTimeSlotIndex != null && block.StartTimeSlotIndex + block.TimeSlotsNeeded > timeSlotIndex)
                current.PowerCapacity -= block.PowerConsumption;
        for (var i = 0; i < blocks.Length; i++)
            if (blocks[i].StartTimeSlotIndex == null)
                // If the block can be started at the current time slot or it is the last time slot where it can be started
                if (blocks[i].PowerConsumption <= current.PowerCapacity ||
                    blocks[i].TimeSlotsNeeded + timeSlotIndex == timeSlotsTotal)
                {
                    blocks[i].StartTimeSlotIndex = timeSlotIndex;
                    current.PowerCapacity -= blocks[i].PowerConsumption;
                }

        return blocks;
    }

    /// <summary>
    ///     Is a block representation of an appliance consumption.
    /// </summary>
    public struct Block
    {
        public int Id { get; init; }

        /// <summary>
        ///     Is the number of watt-hours consumed by the appliance in one time slot.
        /// </summary>
        public double PowerConsumption;

        /// <summary>
        ///     Is the number of time slots the appliance needs to run.
        /// </summary>
        public int TimeSlotsNeeded;

        /// <summary>
        ///     Is the index of the time slot where the appliance starts running.
        /// </summary>
        public int? StartTimeSlotIndex;
    }
}