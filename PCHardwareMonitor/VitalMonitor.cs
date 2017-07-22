using System;
using System.Windows.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace PCHardwareMonitor
{
    class VitalMonitor : IDisposable
    {
        private List<DispatcherTimer> activeDispatchTimers = new List<DispatcherTimer>();
        public static Computer currentPC = new Computer();

        private GPUReport gpuReport;
        private CPUReport cpuReport;

        public VitalMonitor()
        {
            gpuReport = new GPUReport(currentPC);
            cpuReport = new CPUReport(currentPC);
            Action gpuUpdateTask = () => { gpuReport.Update(); };
            Action cpuUpdateTask = () => { cpuReport.Update(); };
            InvokeEverySeconds(cpuUpdateTask, 1);
            InvokeEverySeconds(gpuUpdateTask, 2);
        }

        public static string GetCPUName()
        {
            foreach(var hardware in currentPC.Hardware)
            {
                if (hardware.HardwareType == HardwareType.CPU) { return hardware.Name; }
            }
            return "CPU";
        }

        public static string GetGPUName() { return new GPUReport(currentPC).name; }

        public static int GetCPUCoreCount()
        {
            int coreCount = 0;
            foreach (var hardware in currentPC.Hardware)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.Name.Contains("CPU Core")) { coreCount++; }
                }
            }
            return coreCount;
        }

        public void ListenToCPUUsage(VitalIndicator indicator)
        {
            var text = (string)indicator.label.Content;
            Action task = () => {
                var usage = GetCPUUsage();
                var usageText = text + usage + "%";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usage); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            InvokeEverySeconds(task, 2);
        }

        public void ListenToCPUCoreUsage(VitalIndicator[] indicators)
        {
            Action task = () => {
                var coreLoads = cpuReport.coreLoads;
                if (indicators.Length != coreLoads.Length) { Console.WriteLine("CORE LOADS Doesn't match indicator count"); return; }
                for (int i = 0; i < indicators.Length; i++)
                {
                    var indicator = indicators[i];
                    var load = coreLoads[i];
                    Action updateUI = () => { indicator.UpdateIndicator($"CPU {i + 1}: {load}%", load); };
                    indicator.Dispatcher.Invoke(updateUI);
                }
            };
            task();
            InvokeEverySeconds(task, 1);
        }

        public void ListenToGPULoad(VitalIndicator indicator)
        {
            var text = (string)indicator.label.Content;
            Action task = () => {
                var usage = gpuReport.load;
                var usageText = text + usage + "%";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usage); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 2);
        }
        public void ListenToGPUTemp(VitalIndicator indicator)
        {
            var text = (string)indicator.label.Content;
            Action task = () => {
                var tempf = gpuReport.tempFahrenheit;
                var tempPercent = ((tempf / 220) * 100);
                var usageText = text + tempf + "F";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, tempPercent); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 2);
        }
        public void ListenToGPUMemoryLoad(VitalIndicator indicator)
        {
            var text = (string)indicator.label.Content;
            Action task = () => {
                var usage = (int)gpuReport.memoryLoad;
                var usageText = text + usage + "%";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usage); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 2);
        }
        public void ListenToGPUFanSpeed(VitalIndicator indicator)
        {
            var text = (string)indicator.label.Content;
            Action task = () => {
                var usage = gpuReport.fanSpeed;
                var usagePercent = (gpuReport.fanSpeed / 100);
                var usageText = text+ usage + " RPM";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usagePercent); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 2);
        }

        public void ListenToRAMUsage(VitalIndicator indicator)
        {
            var text = (string)indicator.label.Content;
            Action task = () => {
                var usage = GetRAMUsage();
                var usageText = text + usage + "%";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usage); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 2);
        }

        public void ListenToDrive(VitalIndicator indicator, DriveInfo drive)
        {
            var driveLabel = drive.Name;
            driveLabel = driveLabel.Replace(@":\", "") + " :    ";

            long driveCapacity = drive.TotalSize;
            long freeSpace = drive.TotalFreeSpace;
            var usedSpace = ((double)driveCapacity - (double)freeSpace);
            var decimalPercentUsed = ((((double)usedSpace / (double)driveCapacity) * 100) - 100);
            var percentUsed = (int)Math.Abs(decimalPercentUsed);

            Action task = () => {
                var usedText = driveLabel + percentUsed + "%";
                Action updateUI = () => { indicator.UpdateIndicator(usedText, percentUsed); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 120);
        }

        private int GetRAMUsage()
        {
            Int64 phav = MemoryInformation.GetPhysicalAvailableMemoryInMiB();
            Int64 tot = MemoryInformation.GetTotalMemoryInMiB();
            decimal percentFree = ((decimal)phav / (decimal)tot) * 100;
            decimal percentOccupied = 100 - percentFree;
            return (int)percentOccupied;
        }

        private int GetCPUUsage()
        {
            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(300);
            return (int)cpuCounter.NextValue();
        }

        private double[] GetCPUCoreUsage() { return new CPUReport(currentPC).coreLoads; }

        private void InvokeEverySeconds(Action action, int seconds)
        {
            Action<object, EventArgs> task = delegate (object sender, EventArgs e) { action(); };
            var dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += new EventHandler(task);
            dispatchTimer.Interval = new TimeSpan(0, 0, seconds);
            activeDispatchTimers.Add(dispatchTimer);
            dispatchTimer.Start();
        }

        public void Dispose()
        {
            foreach (var timer in activeDispatchTimers)
                timer.Stop();
        }

        /// <summary>
        /// Doesn't work
        /// </summary>
        /// <param name="indicator"></param>
        /*private void ListenToCPUTemp(VitalIndicator indicator)
        {
            Action task = () => {
                var usage = GetCPUTemp();//GetCPUTempFahrenheit();
                var usageText = "Temp " + usage + "F";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usage); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 2);
        }*/

        /// <summary>
        /// Doesn't work
        /// </summary>
        /// <param name="indicator"></param>
        /*public void ListenToCPUClockSpeed(VitalIndicator indicator)
        {
            Action task = () => {
                var maxClockSpeedMhz = GetMaxCPUClockSpeed();
                var currentClockSpeedMhz = GetCPUClockSpeed();
                double maxClockSpeedGhz = ((double)maxClockSpeedMhz / 1000.0);
                double currentSpeedGhz = ((double)currentClockSpeedMhz / 1000.0);
                int usagePercent = (int)(((double)currentClockSpeedMhz / (double)maxClockSpeedMhz) * 100.0);
                var usageText = "Speed " + currentSpeedGhz + "Ghz ";
                Action updateUI = () => { indicator.UpdateIndicator(usageText, usagePercent); };
                indicator.Dispatcher.Invoke(updateUI);
            };
            task();
            InvokeEverySeconds(task, 1);
        }*/

        /*private int GetCPUClockSpeed()
        {
            int cpuClockSpeed = 0;
            ManagementClass mgmt = new ManagementClass("Win32_Processor");
            ManagementObjectCollection objCol = mgmt.GetInstances();
            foreach (ManagementObject obj in objCol)
            {
                if (cpuClockSpeed == 0)
                {
                    cpuClockSpeed = Convert.ToInt32(obj.Properties["CurrentClockSpeed"].Value.ToString());
                }
            }
            return cpuClockSpeed;
        }*/

        /*private uint GetMaxCPUClockSpeed()
        {
            uint clockSpeed = 0;
            var searcher = new ManagementObjectSearcher("select MaxClockSpeed from Win32_Processor");
            foreach (var item in searcher.Get()) { clockSpeed = (uint)item["MaxClockSpeed"]; }
            return clockSpeed;
        }*/

        /// <summary>
        /// Don't think this is accurate.
        /// </summary>
        /// <returns></returns>
        /*private int GetCPUTempFahrenheit()
        {
            PerformanceCounter perfTempZone = new PerformanceCounter("Thermal Zone Information", "Temperature", @"\_TZ.TZ00");
            var nextValue = perfTempZone.NextValue();
            var fahrenheit = (double)nextValue * 9 / 5 - 459.67;
            return (int)fahrenheit;
        }*/

        /// <summary>
        /// Don't think this is accurate.
        /// </summary>
        /// <returns></returns>
        /*private double GetCPUTemp()
        {
            double tempFahrenheit = 0;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                ManagementObjectSearcher test = new ManagementObjectSearcher();

                foreach (ManagementObject obj in searcher.Get())
                {
                    var tempProperty = obj["CurrentTemperature"];
                    Double tempKelvin = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                    var tempCelcius = (tempKelvin - 2732) / 10.0;
                    tempFahrenheit = (double)tempKelvin * 9.0 / 5.0 - 459.67;
                    return tempFahrenheit;
                }
            }
            catch (System.Exception ex) { Console.WriteLine(ex); }
            return tempFahrenheit;
        }*/
    }

    static class MemoryInformation
    {
        [DllImport("psapi.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPerformanceInfo([Out] out PerformanceInformation PerformanceInformation, [In] int Size);

        [StructLayout(LayoutKind.Sequential)]
        struct PerformanceInformation
        {
            public int Size;
            public IntPtr CommitTotal;
            public IntPtr CommitLimit;
            public IntPtr CommitPeak;
            public IntPtr PhysicalTotal;
            public IntPtr PhysicalAvailable;
            public IntPtr SystemCache;
            public IntPtr KernelTotal;
            public IntPtr KernelPaged;
            public IntPtr KernelNonPaged;
            public IntPtr PageSize;
            public int HandlesCount;
            public int ProcessCount;
            public int ThreadCount;
        }

        internal static Int64 GetPhysicalAvailableMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi))) { return Convert.ToInt64((pi.PhysicalAvailable.ToInt64() * pi.PageSize.ToInt64() / 1048576)); }
            else { return -1; }

        }

        internal static Int64 GetTotalMemoryInMiB()
        {
            PerformanceInformation pi = new PerformanceInformation();
            if (GetPerformanceInfo(out pi, Marshal.SizeOf(pi))) { return Convert.ToInt64((pi.PhysicalTotal.ToInt64() * pi.PageSize.ToInt64() / 1048576)); }
            else { return -1; }
        }
    }
}

