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
    public class DeserializeTest
    {
        public DeserializeTest()
        {

        }

        [Benchmark]
        public void XmlDeserialize()
        {
            TVUPluginList pluginList = TVUXmlSerializable2.DeserializeObjectFromFile<TVUPluginList>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins_Console.xml"), false);
        }

        [Benchmark]
        public void JsonDeserialize()
        {
            TVUPluginList pluginList = TVUJsonSerializable2.DeserializeObjectFromFile<TVUPluginList>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins_Console.json"), false);
        }
    }
}
