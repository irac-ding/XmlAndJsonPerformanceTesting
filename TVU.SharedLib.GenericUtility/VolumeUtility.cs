/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: VolumeUtility.cs
 * Purpose:  calculate volume between db and percentage
 * Author:   Elizabeth added on Oct.12th, 2018.
 * Since:    Microsoft Visual Studio 2015 update3
 * =============================================*/

using System;

namespace TVU.SharedLib.GenericUtility
{
    public class VolumeUtility
    {
        public static int Db2Percentage(double db)
        {
            return db <= -40 ? 0 : Convert.ToInt32(Math.Pow(10, db / 20) * 100);
        }

        public static double Percentage2Db(int rawVolume)
        {
            if (rawVolume <= 1)
                rawVolume = 1;
            if (rawVolume >= 100)
                rawVolume = 100;
            return 20 * Math.Log10(rawVolume / 100.0);
        }
    }
}
