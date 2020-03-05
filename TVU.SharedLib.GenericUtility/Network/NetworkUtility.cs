/* =============================================
 * Copyright 2016 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: NetworkUtility.cs
 * Purpose:  Utility methods for local network related stuff.
 * Author:   MikkoXU added on Nov.30th, 2016.
 * Ref-to:   http://stackoverflow.com/questions/6803073/get-local-ip-address .
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class NetworkUtility
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Methods

        public static bool IsConnected()
        {
            bool ret = NetworkInterface.GetIsNetworkAvailable();
            logger.Debug("IsConnected() returns {0}.", ret);
            return ret;
        }

        public static List<string> GetLocalIPAddresses()
        {
            List<string> ret = new List<string>();

            //IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ifInfo in NetworkInterfaceInfoUtility.NetworkInterfaceInfos)
            {
                if (!string.IsNullOrWhiteSpace(ifInfo.IPv4Address))
                    ret.Add(ifInfo.IPv4Address);
            }
            logger.Debug("GetLocalIPAddresses() returns {0} IPAddresses.", ret.Count);
            return ret;
        }

        /// <summary>
        /// Transform IPv4 maskt to CIDR.
        /// </summary>
        /// <param name="mask">Requires IPv4 format, such as xxx.xxx.xxx.xxx.</param>
        /// <returns>CIDR value.</returns>
        public static int Mask2CIDR(string mask)
        {
            int ret = 0;

            byte[] bytes = IPAddress.Parse(mask).GetAddressBytes();
            foreach (byte b in bytes)
            {
                string sb = Convert.ToString(b, 2);
                int count = sb.Count(c => c == '1');
                ret += count;
            }

            return ret;
        }

        #endregion
    }
}
