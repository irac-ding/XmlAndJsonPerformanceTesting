/*==============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: LibCoreHelper.cs
 * Author:   EvanLI and MikkoXU added 3 methods originally in TVU.TVULibCoreCommon.PInvokePublic.
 *           See r33485, r33696 and r35170.
 * History:  MikkoXU moved to SharedLib on Sept.25th, 2019 to avoid external reference to TVU.TVULibCoreCommon.PInvokePublic.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TVU.SharedLib.GenericUtility
{
    public class LibCoreHelper
    {
        #region UTF-8 Helper

        public static string GetUTF8StringWithEnding(byte[] array)
        {
            int index = Array.IndexOf(array, (byte)0x0);
            if (index == -1)
            {
                index = array.Length;
            }
            return Encoding.UTF8.GetString(array, 0, index);
        }

        public static byte[] GetUTF8Bytes(string str, int length = -1)
        {
            if (str == null) return null;

            return length > 0 ? Encoding.UTF8.GetBytes(str.PadRight(length, '\0')) : Encoding.UTF8.GetBytes(str);
        }

        #endregion

        #region Memory Helper

        //TODO: check(kenny)
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int memcmp(byte[] b1, byte[] b2, UIntPtr count);

        public static bool SequenceEqual(byte[] b1, byte[] b2)
        {
            if (b1 == b2) return true;

            if (b1 == null || b2 == null || b1.Length != b2.Length) return false;

            return memcmp(b1, b2, new UIntPtr((uint)b1.Length)) == 0;
        }

        #endregion
    }
}
