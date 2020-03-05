/* =============================================
 * Copyright 2017 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: NetworkInterfaceUtility.cs
 * Purpose:  Get network interface information in Windows and Linux. This Operator requst user pre-define two connection({interfaceName}-dhcp and {interfacename}-static) for each interface. Each funtion in this class is based on those two connection.
 * Notice:   1. All functions in this class is base on Linux cmd nmcli. Please make sure Linux has already installed NetworkManager rpm package.
 *           2. Before you use this class, please make sure Linux has already created {interfaceName}-dhcp and {interfacename}-static connections by nmcli.
 * Author:   ElizabethHe added on Jul.18th, 2017.
 * See more: For nmcli manual: https://access.redhat.com/documentation/en-US/Red_Hat_Entnderprise_Linux/7/html/Networking_Guide/sec-Using_the_NetworkManager_Command_Line_Tool_nmcli.html or use command `man nmcli` to get more information
 * Since:    Microsoft Visual Studio 2015 update3
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public static class NetworkInterfaceUtility
    {
        #region Log

        private static Logger logger => LogManager.GetCurrentClassLogger();

        #endregion

        private static readonly string NetWorkManagerCommand = "nmcli";
        private static object _cmdLocker { get; } = new object();

        /// <summary>
        /// Please pay attention, this function is Liunx only.
        /// </summary>
        public static void UpInterface(string interfaceName)
        {
            // nmcli dev connect enp4s0

            NetworkInterfaceInfo niInfo = NetworkInterfaceInfoUtility.CheckInterfaceName(interfaceName);
            RunNmcli($"dev connect {interfaceName}");
        }

        /// <summary>
        /// Please pay attention, this function is Liunx only.
        /// </summary>
        public static void DownInterface(string interfaceName)
        {
            // nmcli dev disconnect enp4s0

            NetworkInterfaceInfo niInfo = NetworkInterfaceInfoUtility.CheckInterfaceName(interfaceName);
            RunNmcli($"dev disconnect {interfaceName}");
        }

        public static void UpConnection(string connectionName)
        {
            // nmcli con up enp4s0-dhcp

            RunNmcli($@"con up {connectionName}");
        }

        public static void DownConnection(string connectionName)
        {
            // nmcli con down enp4s0-dhcp

            RunNmcli($@"con down {connectionName}");
        }

        public static void DelConnection(string connectionName)
        {
            // nmcli con del enp4s0-dhcp

            RunNmcli($@"con del {connectionName}");
        }

        public static void AddConnection(string interfaceName, string connectionName)
        {
            // nmcli con add type ethernet con-name enp4s0-dhcp ifname enp4s0

            RunNmcli($@"con add type ethernet con-name {connectionName} ifname {interfaceName}");
        }

        public static void AddConnection(string ifname, bool isdhcp)
        {
            // nmcli con add type ethernet con-name enp4s0-dhcp ifname enp4s0

            string conName = isdhcp ? $"{ifname}-dhcp" : $"{ifname}-static";
            string bashArgs = $"con add type ethernet con-name {conName} ifname {ifname}";
            RunNmcli(bashArgs);
        }

        public static void AddConnectionAndDefaultDNS(string ifname, bool isdhcp)
        {
            string conName = isdhcp ? $"{ifname}-dhcp" : $"{ifname}-static";
            // nmcli con add type ethernet con-name enp4s0-dhcp ifname enp4s0
            string bashArgs = $"con add type ethernet con-name {conName} ifname {ifname}";
            RunNmcli(bashArgs);
            Thread.Sleep(100);

            // nmcli con mod enp4s0-dhcp +ipv4.dns "8.8.8.8 8.8.4.4 114.114.114.114"
            // 8888 8844 for global 
            // 114 for China
            bashArgs = $@"con mod {conName} +ipv4.dns ""8.8.8.8 8.8.4.4 114.114.114.114""";
            RunNmcli(bashArgs);
        }

        public static void DeleteConnection(string ifname, bool isdhcp)
        {
            // nmcli con del con-name enp4s0-dhcp

            string conName = isdhcp ? $"{ifname}-dhcp" : $"{ifname}-static";
            string bashArgs = $"con del con-name {conName}";
            RunNmcli(bashArgs);
        }

        /// <summary>
        /// Please pay attention, this function is Liunx only.
        /// Set dns server address
        /// To set two IPv4 DNS server addresses:
        /// $ nmcli con mod test-lab ipv4.dns "8.8.8.8 8.8.4.4"
        /// to add additional DNS servers to any previously set, use the + prefix as follows:
        /// $ nmcli con mod test-lab +ipv4.dns "8.8.8.8 8.8.4.4"
        /// </summary>
        /// <param name="connectionName">connection name </param>
        /// <param name="dnsList">dns server list</param>
        public static void SetIPv4Dns(string connectionName, List<string> dnsList)
        {
            // nmcli con mod enp4s0-dhcp +ipv4.dns "8.8.8.8 8.8.4.4 114.114.114.114"

            string dnsStr = dnsList.Aggregate(string.Empty, (current, dns) => current.Trim('"') + " " + dns.Trim('"')).Trim();
            RunNmcli($@"con mod {connectionName} ipv4.dns ""{dnsStr}""");
        }

        /// <summary>
        /// Please pay attention, this function is Liunx only.
        /// ADDING A STATIC ETHERNET CONNECTION
        /// step 1: down and remove old network connection, because of nm allow u to storge network connection config with the same conntection name
        /// step 2: add and up a new network connection config 
        /// </summary>
        /// <param name="connectionName">connection name </param>
        /// <param name="ipv4IPAndMask">ipv4 static ip address with subnet mask, format: 10.12.13.14/24. For how to convert subnet mask: https://zh.wikipedia.org/wiki/%E6%97%A0%E7%B1%BB%E5%88%AB%E5%9F%9F%E9%97%B4%E8%B7%AF%E7%94%B1#.E5.89.8D.E7.BC.80.E8.81.9A.E5.90.88 </param>
        /// <param name="ipv4Gateway">ipv4 gateway address</param>
        public static void SetupNetworkConfig(string connectionName, string ipv4IPAndMask, string ipv4Gateway)
        {
            //nmcli con mod em1-1 ipv4.method manual ipv4.addr "192.168.1.2/24, 10.10.1.5/8"

            RunNmcli($@"con mod {connectionName} ipv4.method manual ipv4.addr {ipv4IPAndMask} ipv4.gateway {ipv4Gateway}");
            RunNmcli($@"con up {connectionName}");
        }

        /// <summary>
        /// Please pay attention, this function is Liunx only.
        /// nmcli con mod eth0-static connection.autoconnect yes
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="isAutoConnect"></param>
        public static void SetupConnectionAutoConnect(string connectionName, bool isAutoConnect)
        {
            //nmcli con mod enp4s0-dhcp connection.autoconnect yes

            string autoConStr = isAutoConnect ? "yes" : "no";
            RunNmcli($@"con mod {connectionName} connection.autoconnect {autoConStr}");
        }

        private static void RunNmcli(string bashCommand)
        {
            logger.Info($"RunNmCli() FileName is {NetWorkManagerCommand}, Arguments is {bashCommand}.");
            if (!SystemConfigurationInfo.IsWindows)
            {
                if (Monitor.TryEnter(_cmdLocker, 2000))
                {
                    try
                    {
                        Process proc = new Process();
                        proc.StartInfo.FileName = $"{NetWorkManagerCommand}";
                        proc.StartInfo.Arguments = $"{bashCommand}";
                        proc.StartInfo.UseShellExecute = false;

                        proc.StartInfo.RedirectStandardOutput = true;
                        proc.StartInfo.RedirectStandardError = true;
                        proc.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                        proc.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);
                        proc.Start();
                        proc.BeginOutputReadLine();
                        proc.BeginErrorReadLine();
                        proc.WaitForExit(1000);
                        if (proc.HasExited)
                            Console.WriteLine($@"""{NetWorkManagerCommand} {bashCommand}"" exit code: {proc.ExitCode}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        logger.Error($"RunNmcli() error: {ex.Message}");
                    }
                    finally
                    {
                        Monitor.Exit(_cmdLocker);
                    }
                }
            }
        }
    }
}
