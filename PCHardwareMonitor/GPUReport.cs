using System;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace PCHardwareMonitor
{
    class GPUReport
    {
        public double tempCelsius;
        public double tempFahrenheit;
        public String name;
        public int load;
        public int fanSpeed;
        public double memoryLoad;
        private Computer pc;

        public GPUReport(Computer pc)
        {
            this.pc = pc;
            pc.GPUEnabled = true;
            pc.Open();
            Update();
        }

        public void Update()
        {
            foreach (var hardware in pc.Hardware)
            {
                hardware.Update();
                if (hardware.HardwareType != HardwareType.GpuAti && hardware.HardwareType != HardwareType.GpuNvidia) { continue; }
                name = hardware.Name;
                foreach (var sensor in hardware.Sensors)
                {
                    switch (sensor.SensorType)
                    {
                        case SensorType.Temperature:
                            tempCelsius = (double)sensor.Value;
                            tempFahrenheit = tempCelsius * 9.0 / 5.0 + 32.0;
                            break;
                        case SensorType.Load:
                            switch (sensor.Name)
                            {
                                case "GPU Memory": this.memoryLoad = (double)sensor.Value; break;
                                case "GPU Core": this.load = (int)sensor.Value; break;
                            }
                            break;
                        case SensorType.Fan:
                            if (sensor.Name == "GPU") { fanSpeed = (int)sensor.Value; }
                            break;
                    }
                }
            }
        }
    }
}
/*
    HARDWARE : NVIDIA GeForce GTX 1070; GpuNvidia
    GPU Core OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Temperature = 32
    GPU OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Fan = 1092
    GPU Core OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Clock = 1506
    GPU Memory OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Clock = 4006.8
    GPU Shader OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Clock = 3012
    GPU Core OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Load = 1
    GPU Memory Controller OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Load = 0
    GPU Video Engine OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Load = 0
    GPU Fan OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Control = 27
    GPU Memory OpenHardwareMonitor.Hardware.Nvidia.NvidiaGPU Load = 5.461264
 */
