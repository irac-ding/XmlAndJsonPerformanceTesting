/* =============================================
 * Copyright 2017 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: AppInit.cs
 * Purpose:  Get network interface information data module.
 * Author:   ElizabethHe added on Jul.18th, 2017.
 * Since:    Microsoft Visual Studio 2015 update3
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class NetworkInterfaceInfo
    {

        #region Log
        private static Logger logger = LogManager.GetCurrentClassLogger();
        #endregion

        public string Name { get; internal set; } = string.Empty;
        public int Order { get; internal set; }
        public NetworkInterfaceType InterfaceType { get; set; }
        public string MacAddress { get; internal set; } = string.Empty;
        public bool IsDhcpEnabled { get; internal set; }
        public bool IsUp { get; internal set; }
        public bool IsPluginCable { get; internal set; }
        public string IPv4Address { get; internal set; } = string.Empty;
        public string IPv4Mask { get; internal set; } = string.Empty;
        public string IPv4GatewayAddress { get; internal set; } = string.Empty;
        public List<string> IPv4DnsAddresses { get; internal set; } = new List<string>();

        internal NetworkInterface Ni { get; }

        internal NetworkInterfaceInfo(NetworkInterface ni)
        {
            Ni = ni;
            ReloadInterfaceInfo();
        }

        public void ReloadInterfaceInfo()
        {
            IPInterfaceProperties p = Ni.GetIPProperties();

            Name = Ni.Name;
            InterfaceType = Ni.NetworkInterfaceType;
            MacAddress = NetworkInterfaceInfoUtility.ConverterMacAddress(Ni.GetPhysicalAddress());
            // In Linux https://github.com/n4t/mono/blob/master/mcs/class/System/System.Net.NetworkInformation/IPv4InterfaceProperties.cs , mono doesn't implement IsDhcpEnabled this properties.
            if (SystemConfigurationInfo.IsWindows)
                IsDhcpEnabled = p.GetIPv4Properties().IsDhcpEnabled;
            else
                IsDhcpEnabled = NetworkInterfaceInfoUtility.QueryInterfaceIsDHCPByConnection($"{Ni.Name}-dhcp");
            IsUp = Ni.OperationalStatus == OperationalStatus.Up;
            IsPluginCable = NetworkInterfaceInfoUtility.QueryInterfaceIsPlugined(Ni.Name);

            foreach (UnicastIPAddressInformation ip in p.UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IPv4Address = ip.Address.ToString();
                    IPv4Mask = ip.IPv4Mask.ToString();
                }
            }

            foreach (GatewayIPAddressInformation gateway in p.GatewayAddresses)
            {
                if (gateway.Address.AddressFamily == AddressFamily.InterNetwork)
                    IPv4GatewayAddress = gateway.Address.ToString();
            }

            string dnsListStr = string.Empty;

            foreach (IPAddress dns in p.DnsAddresses)
            {
                if (dns != null)
                {
                    if (!SystemConfigurationInfo.IsWindows)
                    {
                        dnsListStr = string.IsNullOrWhiteSpace(dnsListStr) ? dns.ToString() : $"{Environment.NewLine}{dns}";
                        IPv4DnsAddresses.Add(dns.ToString());
                    }
                    else
                    {
                        dnsListStr = string.IsNullOrWhiteSpace(dnsListStr) ? dns.MapToIPv4().ToString() : $"{Environment.NewLine}{dns.MapToIPv4()}";
                        IPv4DnsAddresses.Add(dns.MapToIPv4().ToString());
                    }
                }

            }

            logger.Info($"NetworkInterfaceInformation: Name: {Name}, MacAddress: {MacAddress}, IsDhcpEnabled: {IsDhcpEnabled}, IsUp: {IsUp}, IsPluginCable: {IsPluginCable}, IPv4Address: {IPv4Address}, IPv4Mask: {IPv4Mask}, IPv4Gateway: {IPv4GatewayAddress}, DNSMapToIPv4: {dnsListStr}");

            if (!SystemConfigurationInfo.IsWindows)
            {
                Console.WriteLine($@"NetworkInterfaceInformation:
{{    
    Name: {Name},
    MacAddress: {MacAddress},
    IsDhcpEnabled: {IsDhcpEnabled},
    IsUp: {IsUp},
    IsPluginCable: {IsPluginCable},
    IPv4Address: {IPv4Address},
    IPv4Mask: {IPv4Mask},
    IPv4Gateway: {IPv4GatewayAddress},
    DNSMapToIPv4: {dnsListStr},
}}");
            }
        }
    }
}
