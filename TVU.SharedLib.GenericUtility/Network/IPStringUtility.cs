/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: IPStringUtility.cs
 * Purpose:  Utility methods for converting IPv4 address between string and UInt32.
 * Author:   ElizabethHe added on Dec.17th, 2015. The logic was in Utility.MyUtil.cs.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Net;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class IPStringUtility
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        /// <summary>
        /// Convert IPv4 address from string to UInt32.
        /// Eg: 10.12.32.169 => 2837449738.
        /// </summary>
        /// <param name="ipInString">IPv4 address in string.</param>
        /// <returns>IPv4 address in UInt32.</returns>
        public static uint StringToUInt32(string ipInString)
        {
            try
            {
                return BitConverter.ToUInt32(IPAddress.Parse(ipInString).GetAddressBytes(), 0);
            }
            catch (Exception ex)
            {
                logger.Error("StringToUInt32() error, {0}.", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Convert IPv4 address from UInt32 to string.
        /// Eg: 2837449738 => 10.12.32.169.
        /// </summary>
        /// <param name="ipInUInt32">IPv4 address in UInt32.</param>
        /// <returns>IPv4 address in string.</returns>
        public static string UInt32ToString(uint ipInUInt32)
        {
            return new IPAddress(ipInUInt32).ToString();
        }
    }
}
