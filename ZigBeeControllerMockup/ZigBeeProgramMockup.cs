﻿using System.Diagnostics;
using IoTControllerContracts;

namespace ZigBeeControllerMockup
{
    internal class ZigBeeProgramMockup : IProgram
    {
        private ProgramStatus _status = ProgramStatus.Available;
        private readonly Stopwatch _stopwatch = new();
        private Thread _runtimeThread;
        private readonly ZigBeeApplianceMockup _appliance;

        public ZigBeeProgramMockup(string name, double powerConsumptionInWattHours, int runTimeInMinutes, ZigBeeApplianceMockup appliance)
        {
            Name = name;
            PowerConsumptionInWattHours = powerConsumptionInWattHours;
            RunTimeInMinutes = runTimeInMinutes;
            _appliance = appliance;
        }

        public string Name { get; }
        public double PowerConsumptionInWattHours { get; }
        public int RunTimeInMinutes { get; }

        public IAppliance Appliance => _appliance;

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
                _stopwatch.Restart();
                _runtimeThread = new Thread(() =>
                {
                    while (_status == ProgramStatus.Running)
                    {
                        lock (this)
                        {
                            if (_stopwatch.Elapsed.TotalMinutes >= RunTimeInMinutes)
                            {
                                _status = ProgramStatus.Unavailable;
                                _stopwatch.Stop();
                                _appliance.SetAvailable();
                            }
                        }

                        Thread.Sleep(1000);
                    }
                });
                _runtimeThread.Start();
                _status = ProgramStatus.Running;
                _appliance.SetUnavailable();
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
                _appliance.SetAvailable();
            }
            return Task.FromResult(true);
        }

        public bool MakeAvailable()
        {
            if (_status == ProgramStatus.Running)
                return false;
            _status = ProgramStatus.Available;
            return true;
        }

        public bool MakeUnavailable()
        {
            if (_status == ProgramStatus.Running)
                return false;
            _status = ProgramStatus.Unavailable;
            return true;
        }
    }
}
