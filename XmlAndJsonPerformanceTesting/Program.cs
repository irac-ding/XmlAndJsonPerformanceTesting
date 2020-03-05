using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using TVU.SharedLib.Json;
using TVU.SharedLib.Xml;

namespace XmlAndJsonPerformanceTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var deserializeSummary = BenchmarkRunner.Run<DeserializeTest>();
            var serializeSummary = BenchmarkRunner.Run<SerializeTest>();
            Console.ReadLine();

        }
        
    }

}
