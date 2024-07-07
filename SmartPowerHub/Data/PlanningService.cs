using MudBlazor;
using System.Data;
using System.Security.AccessControl;
using Google.OrTools.LinearSolver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constraint = Google.OrTools.LinearSolver.Constraint;

namespace SmartPowerHub.Data
{
    struct Block
    {
        public int Id { get; init; }
        public int PowerConsumption;
        public int TimeSlotsNeeded;
        public int? StartTimeSlotIndex;
    }
    struct TimeSlot
    {
        public double PowerCapacity;
    }
    public class PlanningService
    {
        public async Task<(DateTime time, double prediction)[]> GetPlan()
        {
            var plan = new (DateTime, double)[]
            {
                (DateTime.Now, 5),
                (DateTime.Now.AddHours(1), 10),
                (DateTime.Now.AddHours(2), 30),
                (DateTime.Now.AddHours(3), 60),
                (DateTime.Now.AddHours(4), 120),
                (DateTime.Now.AddHours(5), 240),
                (DateTime.Now.AddHours(6), 480),
                (DateTime.Now.AddHours(7), 1000),
                (DateTime.Now.AddHours(8), 1500),
                (DateTime.Now.AddHours(9), 2000),
                (DateTime.Now.AddHours(10), 2250),
                (DateTime.Now.AddHours(11), 2400),
                (DateTime.Now.AddHours(12), 2400),
                (DateTime.Now.AddHours(13), 2250),
                (DateTime.Now.AddHours(14), 2000),
                (DateTime.Now.AddHours(15), 1500),
                (DateTime.Now.AddHours(16), 1000),
                (DateTime.Now.AddHours(17), 480),
                (DateTime.Now.AddHours(18), 240),
                (DateTime.Now.AddHours(19), 120),
                (DateTime.Now.AddHours(20), 60),
                (DateTime.Now.AddHours(21), 30),
                (DateTime.Now.AddHours(22), 10),
                (DateTime.Now.AddHours(23), 5),
            };

            return plan;
        }

        Block[] PlanBlocks(Block[] blocks, TimeSlot[] timeSlots)
        {
            //Solver milp_solver = Solver.CreateSolver("CBC_MIXED_INTEGER_PROGRAMMING");
            Solver milp_solver = Solver.CreateSolver("SCIP");

            //Variables indicating power overflows in each time slot
            Variable[] tsOverflowVars = milp_solver.MakeNumVarArray(
                timeSlots.Length, 0, double.PositiveInfinity, "TimeSlotOverflows");

            //Offset time slots to prevent negative indexes set to the maximum number of time slots needed by an appliance
            int offset = blocks.Max(b => b.TimeSlotsNeeded);

            //2-D array where each variable means if an appliance starts at a time slot
            Variable[,] appliancesVars = new Variable[blocks.Length, timeSlots.Length + offset];

            //Create variables for each appliance and time slot
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < timeSlots.Length + offset; j++)
                {
                    appliancesVars[i, j] = milp_solver.MakeBoolVar("Appliance" + i + "TimeSlot" + (j - offset));
                }
            }

            //Constraint -1: First starting time slot of each appliance must be at least the time slot on offset index.
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = 0; j < offset; j++)
                {
                    milp_solver.Add(appliancesVars[i, j] == 0);
                }
            }


            //Constraint 0: Last starting time slot of each appliance must be at most the last time slot minus the number
            //of time slots the appliance needs
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = timeSlots.Length - blocks[i].TimeSlotsNeeded + 1; j < timeSlots.Length; j++)
                {
                    milp_solver.Add(appliancesVars[i, j + offset] == 0);
                }
            }

            //Constraint 1: Each appliance must start exactly once
            for (int i = 0; i < blocks.Length; i++)
            {
                Constraint c1 = milp_solver.MakeConstraint(1, 1);
                for (int j = 0; j < timeSlots.Length + offset; j++)
                {
                    c1.SetCoefficient(appliancesVars[i, j], 1);
                }
            }

            //Constraint 2: In each time slot the overflow variable is greater or equal to the sum of power consumptions
            //of appliances that are running in that time slot minus the power capacity of that time slot. And each
            //appliance must run in consecutive time slots for the number of time slots it needs.
            for (int j = offset; j < timeSlots.Length + offset; j++)
            {
                Constraint c2 = milp_solver.MakeConstraint(double.NegativeInfinity, timeSlots[j - offset].PowerCapacity);
                for (int i = 0; i < blocks.Length; i++)
                {
                    for (int k = 0; k < blocks[i].TimeSlotsNeeded; k++)
                    {
                        c2.SetCoefficient(appliancesVars[i, j - k], blocks[i].PowerConsumption);
                    }
                }
                c2.SetCoefficient(tsOverflowVars[j - offset], -1);
            }

            //Objective function: Minimize the sum of overflow variables
            Objective objective = milp_solver.Objective();
            for (int j = 0; j < timeSlots.Length; j++)
            {
                objective.SetCoefficient(tsOverflowVars[j], 1);
            }
            objective.SetMinimization();

            //Solve the problem
            var resultStatus = milp_solver.Solve();

            if (resultStatus != Solver.ResultStatus.OPTIMAL)
            {
                throw new Exception("The problem does not have an optimal solution!");
            }

            //Return planned blocks
            var plannedBlocks = new Block[blocks.Length];
            for (int i = 0; i < blocks.Length; i++)
            {
                for (int j = offset; j < timeSlots.Length + offset; j++)
                {
                    if (appliancesVars[i, j].SolutionValue() == 1)
                    {
                        plannedBlocks[i] = new Block
                        {
                            Id = blocks[i].Id,
                            PowerConsumption = blocks[i].PowerConsumption,
                            TimeSlotsNeeded = blocks[i].TimeSlotsNeeded,
                            StartTimeSlotIndex = j - offset
                        };
                    }
                }
            }

            return plannedBlocks;
        }
    }
}
