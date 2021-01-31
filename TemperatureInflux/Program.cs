using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            var moduleName = ConfigurationManager.AppSettings["ModuleName"];
            var tags = $"host={System.Net.Dns.GetHostEntry("").HostName},instance={instance.Replace(" ", "\\ ")}";
            var field = string.Join(",", fields.Select(f => $"{f.Item1.Replace(" ", "\\ ")}={f.Item2.Replace(" ", "\\ ")}"));
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() * 1000000;
            Console.WriteLine($"{moduleName},{tags} {field} {timestamp}");
        }

        private static void GetConfiguredSystemInfo()
        {
            var updateVisitor = new UpdateVisitor();
            var computer = new Computer();
            computer.Open();
            setModules(computer);
            computer.Accept(updateVisitor);
            var sensors = GetAllSensors(computer.Hardware);
            var filtered = FilterSensors(sensors);
            var groupedByHW = filtered.GroupBy(s => s.Hardware.Name);
            foreach (var hwGroup in groupedByHW)
            {
                var values = hwGroup
                        .Select(s => Tuple.Create(s.Name, s.Value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? String.Empty));
                WriteMetric(hwGroup.Key, values);
            }

            computer.Close();
        }

        private static IEnumerable<IHardware> FlattenHardware(IHardware hardware)
        {
            var subHardware = hardware.SubHardware.SelectMany(s => FlattenHardware(s));
            return new[] { hardware }.Union(subHardware);
        }

        private static IEnumerable<ISensor> GetAllSensors(IEnumerable<IHardware> hardware)
        {
            var allHardware = hardware.SelectMany(h => FlattenHardware(h));
            return allHardware.SelectMany(h => h.Sensors);
        }

        private static Computer setModules(Computer computer)
        {
            var settings = ConfigurationManager.AppSettings["EnabledModules"];
            var modules = settings.Split(',').Select(s => s.ToLower());
            computer.CPUEnabled = modules.Contains("cpu");
            computer.MainboardEnabled = modules.Contains("mobo");
            computer.RAMEnabled = modules.Contains("ram");
            computer.HDDEnabled = modules.Contains("hdd");
            computer.FanControllerEnabled = modules.Contains("fan");
            computer.GPUEnabled = modules.Contains("gpu");
            return computer;
        }

        private static IEnumerable<ISensor> FilterSensors(IEnumerable<ISensor> sensors)
        {

            var settings = ConfigurationManager.AppSettings["Sensors"];
            var sensorsToShow = settings.Split(',');
            return sensors.Where(s => sensorsToShow.Contains(s.Identifier.ToString()));
        }

        private static void ListOptions()
        {
            var updateVisitor = new UpdateVisitor();
            var computer = new Computer();
            computer.Open();
            computer.CPUEnabled = true;
            computer.MainboardEnabled = true;
            computer.RAMEnabled = true;
            computer.HDDEnabled = true;
            computer.FanControllerEnabled = true;
            computer.GPUEnabled = true;
            computer.Accept(updateVisitor);
            var sensors = GetAllSensors(computer.Hardware);
            Console.WriteLine("Identifier                    Name                Value     ");
            Console.WriteLine("----------------------------- ------------------- ----------");
            foreach (var sensor in sensors)
            {
                Console.WriteLine($"{sensor.Identifier,-30} {sensor.Name,-20 } {sensor.Value,-10}");
            }
        }

        private static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "list")
                ListOptions();
            else
                GetConfiguredSystemInfo();
#if DEBUG
            Console.WriteLine($"Press enter to exit...");
            Console.ReadLine();
#endif
        }
    }
}