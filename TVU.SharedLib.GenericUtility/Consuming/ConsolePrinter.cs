using System;

namespace TVU.SharedLib.GenericUtility.Consuming
{
    public class ConsolePrinter : ILogPrinter
    {
        public void PrintMsg(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
