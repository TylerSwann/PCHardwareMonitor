using System;
using System.IO;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace PCHardwareMonitor
{
    public class CPUReport
    {
        public string name;
        public int numberofCores;
        public double cpuLoad;
        public double[] coreLoads = new double[] { };
        private Computer pc;

        public CPUReport(Computer pc)
        {
            this.pc = pc;
            pc.CPUEnabled = true;
            pc.HDDEnabled = true;
            pc.Open();
            Update();
        }

        public void Update()
        {
            var coreLoads = new List<double>();
            foreach (var hardware in pc.Hardware)
            {
                if (hardware.HardwareType != HardwareType.CPU) { continue; }
                name = hardware.Name;
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load)
                    {
                        if (sensor.Name.Contains("CPU Core"))
                        {
                            try { coreLoads.Add((double)sensor.Value); }
                            catch (System.Exception ex) { Console.WriteLine(ex); }
                            continue;
                        }
                        if (sensor.Name.Contains("CPU Total"))
                        {
                            try { cpuLoad = (double)sensor.Value; }
                            catch (System.Exception ex) { Console.WriteLine(ex); }
                            continue;
                        }
                    }
                }
            }
            this.coreLoads = coreLoads.ToArray();
            this.numberofCores = coreLoads.Count;
        }
    }
}
