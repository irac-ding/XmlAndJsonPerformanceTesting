/*==============================================
 * Copyright 2014 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: Numbers.cs
 * Purpose:  Utility methods for numbers.
 * Author:   Mikko XU (mikkoxu@tvunetworks.com) added on Feb.13th, 2014.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class Numbers
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region float / ushort

        public static ushort Float2UShort(float f)
        {
            return (ushort)(Math.Round(f, 1) * 1000);
        }

        public static float UShort2Float(ushort u)
        {
            return (float)Math.Round(u / 1000.0, 1);
        }

        #endregion
    }
}
