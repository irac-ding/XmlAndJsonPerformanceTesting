using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using TVU.SharedLib.Json;
using TVU.SharedLib.Xml;

namespace XmlAndJsonPerformanceTesting
{
    [MemoryDiagnoser]
    public class SerializeTest
    {
        public TVUPluginList PluginList { get; set; }
        public SerializeTest()
        {
            PluginList = TVUXmlSerializable2.DeserializeObjectFromFile<TVUPluginList>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins_Console.xml"), false);
        }
        [Benchmark]
        public void XmlSerializable()
        {
             TVUXmlSerializable2.SerializeObjectToFile(PluginList, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid().ToString()}Plugins_Console.xml"), false);
        }

        [Benchmark]
        public void JsonSerializable()
        {
            TVUJsonSerializable2.SerializeObjectToFile(PluginList, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{Guid.NewGuid().ToString()}Plugins_Console.json"), false);
        }
    }

}
