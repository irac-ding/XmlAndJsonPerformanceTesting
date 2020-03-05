/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: HexUtility.cs
 * Purpose:  Utility methods for Hex.
 * Author:   MikkoXU added on Sept.29th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Globalization;
using System.Linq;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class HexUtility
    {
        public static bool IsX16Valid(string id)
        {
            ulong temp;
            return ulong.TryParse(id, NumberStyles.HexNumber, null, out temp);
        }

        public static void ParseOrFireException(string idHex, string parameterName, out ulong id)
        {
            if (!ulong.TryParse(idHex, NumberStyles.HexNumber, null, out id))
                throw new ArgumentOutOfRangeException(parameterName);
        }

        /// <summary>
        /// Translate FourCC ASCII string to hex string.
        /// </summary>
        public static string AsciiToHex(string ascii)
        {
            string ret = string.Empty;

            string[] chars = ascii.Select(s => string.Format("{0:X2}", Convert.ToUInt32(s))).ToArray();
            foreach (string c in chars)
            {
                ret += c;
            }

            return ret;
        }
    }
}
