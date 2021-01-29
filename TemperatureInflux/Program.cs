using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TemperatureInflux
{
    internal class Program
    {
        public class UpdateVisitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }
            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();
                foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
            }
            public void VisitSensor(ISensor sensor) { }
            public void VisitParameter(IParameter parameter) { }
        }

        private static void WriteMetric(string instance, IEnumerable<Tuple<string, string>> fields)
        {
            var tags = $"host={System.Net.Dns.GetHostEntry("").HostName},instance={instance.Replace(" ", "\\ ")}";
            var field = string.Join(",", fields.Select(f => $"{f.Item1.Replace(" ", "\\ ")}={f.Item2.Replace(" ", "\\ ")}"));
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000;
            Console.WriteLine($"win_temp,{tags} {field} {timestamp}");
        }

        private static void GetSystemInfo()
        {
            var updateVisitor = new UpdateVisitor();
            var computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.MainboardEnabled = true;
            computer.Accept(updateVisitor);
            for (int i = 0; i < computer.Hardware.Length; i++)
            {
                if (computer.Hardware[i].HardwareType == HardwareType.CPU)
                {
                    var cpuTemps = computer.Hardware[i].Sensors
                        .Where(s => s.SensorType == SensorType.Temperature)
                        .Select(s => Tuple.Create(s.Name, s.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    WriteMetric("CPU Temp", cpuTemps);

                }
                if (computer.Hardware[i].HardwareType == HardwareType.Mainboard)
                {
                    var temps = computer.Hardware[i].SubHardware[0].Sensors.Where(s => s.SensorType == SensorType.Temperature)
                        .Select(s => Tuple.Create(s.Name, s.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    WriteMetric("Mobo Temp", temps);
                }
            }
            computer.Close();
        }

        private static void Main(string[] args)
        {

            GetSystemInfo();
#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}