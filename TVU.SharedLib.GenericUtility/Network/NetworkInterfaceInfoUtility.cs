/* =============================================
 * Copyright 2017 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: AppInit.cs
 * Purpose:  Get network interface information in Windows and Linux.
 * Author:   ElizabethHe added on Jul.18th, 2017.
 * See more: For subnet mask: https://stackoverflow.com/questions/13901436/how-to-get-subnet-mask-using-net
 *           For IsDhcp enabled: https://msdn.microsoft.com/en-us/library/system.net.networkinformation.ipv4interfaceproperties.isdhcpenabled(v=vs.110).aspx
 *           For mac address: https://stackoverflow.com/questions/1746840/get-mac-address-in-linux-using-mono
 *           For Dns: https://msdn.microsoft.com/en-us/library/system.net.networkinformation.ipinterfaceproperties.dnsaddresses(v=vs.110).aspx
 *           For nmcli manual: https://access.redhat.com/documentation/en-US/Red_Hat_Enterprise_Linux/7/html/Networking_Guide/sec-Using_the_NetworkManager_Command_Line_Tool_nmcli.html or use command `man nmcli` to get more information
 *           For sysfs-class-net: https://www.kernel.org/doc/Documentation/ABI/testing/sysfs-class-net
 * Since:    Microsoft Visual Studio 2015 update3
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public static class NetworkInterfaceInfoUtility
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static List<NetworkInterfaceInfo> NetworkInterfaceInfos { get; private set; } = new List<NetworkInterfaceInfo>();
        private static readonly string NetWorkManagerCommand = "nmcli";
        private static readonly string NetWorkConfigFilePrefix = "/etc/sysconfig/network-scripts/ifcfg-";
        private static readonly string InterfaceCarrieInfoPath = "/sys/class/net/{0}/carrier";

        static NetworkInterfaceInfoUtility()
        {
            ReloadNetworkInterfaceInfo();
        }

        // TODO: Handle concurrency and reload it when NetworkChange.NetworkAddressChanged is fired. Check CM-2086.
        public static void ReloadNetworkInterfaceInfo()
        {
            NetworkInterfaceInfos.Clear();
            int i = 1;
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    NetworkInterfaceInfo niInfo = new NetworkInterfaceInfo(ni);

                    NetworkInterfaceInfos.Add(niInfo);

                    niInfo.Order = i++;
                }
            }

            GetPhysicalNICOrder();
        }

        public static void GetPhysicalNICOrder()
        {
            // copy from: https://serverfault.com/a/833577
            List<string> nicListInOrder = RunBashAndReturnStdout("find", @"/sys/class/net -type l -not -lname '*virtual*' -print");

            string logstr = string.Join("--", nicListInOrder);
            logger.Info($"Physical NIC order: {logstr}");
            Console.WriteLine($"Physical NIC order: {logstr}");

            if (nicListInOrder?.Count > 0)
            {
                for (int i = 0; i < nicListInOrder.Count; i++)
                {
                    NetworkInterfaceInfo ni = NetworkInterfaceInfos.SingleOrDefault(nic => nicListInOrder[i].Contains(nic.Name));
                    if (ni != null)
                        ni.Order = i + 1;
                }
            }
        }

        public static string ConverterMacAddress(PhysicalAddress macAddress)
        {
            byte[] bytes = macAddress.GetAddressBytes();
            string ret = string.Empty;
            // TODO: Use string.Join().
            for (int i = 0; i < bytes.Length; i++)
            {
                // Display the physical address in hexadecimal.
                ret += bytes[i].ToString("X2");
                // Insert a hyphen after each byte, unless we are at the end of the
                // address.
                if (i != bytes.Length - 1)
                    ret += ":";
            }
            return ret;
        }

        public static NetworkInterfaceInfo CheckInterfaceName(string interfaceName)
        {
            NetworkInterfaceInfo niInfo = NetworkInterfaceInfos.SingleOrDefault(o => o.Name == interfaceName);
            if (niInfo == null)
                throw new ArgumentException($"Can not find this interface: {interfaceName}.");
            return niInfo;
        }

        internal static bool QueryInterfaceIsPlugined(string ifName)
        {
            //  /sys/class/net/<iface>/carrier : Indicates the current physical link state of the interface.
            //  see more: https://www.kernel.org/doc/Documentation/ABI/testing/sysfs-class-net
            bool isPlugined = true;
            if (!SystemConfigurationInfo.IsWindows)
            {
                string ifCarrieInfo = string.Format(InterfaceCarrieInfoPath, ifName);
                string ret = File.ReadAllText(ifCarrieInfo);
                logger.Info($"path: {ifCarrieInfo}, ifname: {ifName}, ret: {ret}");
                if (string.IsNullOrWhiteSpace(ret))
                    isPlugined = false;
                else
                    isPlugined = Convert.ToBoolean(Convert.ToInt32(ret.Trim()));
            }
            return isPlugined;
        }

        internal static bool QueryInterfaceIsDHCPByConnection(string conName)
        {
            string conntionFile = $"{NetWorkConfigFilePrefix}{conName}";
            Console.WriteLine($"Connect file: {conntionFile}");
            if (File.Exists(conntionFile))
            {
                using (StreamReader file = new StreamReader(conntionFile))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.StartsWith("ONBOOT"))
                        {
                            if (line.ToLower().Contains("yes"))
                                return true;
                            return false;
                        }
                    }
                    file.Close();
                }
            }
            return false;
        }

        private static string GetCurrentActiveConntion(string ifName)
        {
            string acitveConnetion = string.Empty;
            List<string> retStr = RunBashAndReturnStdout(NetWorkManagerCommand, $"con show --active");
            string activestr = string.Empty;
            retStr.ForEach(o =>
            {
                if (o != null && o.Contains(ifName) && string.IsNullOrWhiteSpace(activestr))
                    activestr = o;
            });
            Console.WriteLine($"Actvie connection str: {activestr}");
            string[] retArr = activestr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (retArr.Length > 0) acitveConnetion = retArr[0];
            return acitveConnetion;
        }

        private static List<string> RunBashAndReturnStdout(string fn, string bashCommand)
        {

            logger.Info($"RunNmCli() FileName is {fn}, Arguments is {bashCommand}.");
            List<string> ret = new List<string>();

            if (!SystemConfigurationInfo.IsWindows)
            {
                using (Process proc = new Process())
                {
                    proc.StartInfo.FileName = $"{fn}";
                    proc.StartInfo.Arguments = $"{bashCommand}";
                    proc.StartInfo.UseShellExecute = false;

                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.Start();
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string line = proc.StandardOutput.ReadLine();
                        Console.WriteLine($"std out: {line}");
                        if (!string.IsNullOrWhiteSpace(line))
                            ret.Add(line);
                    }
                }
            }
            return ret;
        }
    }
}
