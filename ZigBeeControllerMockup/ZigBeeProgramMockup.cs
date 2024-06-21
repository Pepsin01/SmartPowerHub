using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZigBeeControllerMockup
{
    public enum ProgramStatus
    {
        Available,
        Unavailable,
        Running
    }
    public interface IProgram
    {
        string Name { get; }
        int PowerConsumptionInWattHours { get; }
        int RunTimeInMinutes { get; }
        Task<ProgramStatus> GetStatusAsync();
        Task<int> GetRemainingTimeAsync();
        Task<bool> StartAsync();
        Task<bool> TryStopAsync();
    }
    internal class ZigBeeProgramMockup(string name, int powerConsumptionInWattHours, int runTimeInMinutes)
        : IProgram
    {
        private ProgramStatus _status = ProgramStatus.Available;
        private Stopwatch _stopwatch = new Stopwatch();

        public string Name { get; } = name;
        public int PowerConsumptionInWattHours { get; } = powerConsumptionInWattHours;
        public int RunTimeInMinutes { get; } = runTimeInMinutes;

        public Task<ProgramStatus> GetStatusAsync()
        {
            lock (this)
                return Task.FromResult(_status);
        }

        public Task<int> GetRemainingTimeAsync()
        {
            lock (this)
                return _status != ProgramStatus.Running ? Task.FromResult(0) : Task.FromResult(RunTimeInMinutes - (int)_stopwatch.Elapsed.TotalMinutes);
        }

        public Task<bool> StartAsync()
        {
            lock (this)
            {
                if (_status == ProgramStatus.Running)
                    return Task.FromResult(false);
                _stopwatch.Start();
                _status = ProgramStatus.Running;
            }
            return Task.FromResult(true);
        }

        public Task<bool> TryStopAsync()
        {
            lock (this)
            {
                if (_status != ProgramStatus.Running)
                    return Task.FromResult(false);
                _stopwatch.Stop();
                _status = ProgramStatus.Available;
            }
            return Task.FromResult(true);
        }
    }
}
