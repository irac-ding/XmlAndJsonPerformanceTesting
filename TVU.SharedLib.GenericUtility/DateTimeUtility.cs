/* =============================================
 * Copyright 2013 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: DateTimeUtility.cs
 * Purpose:  Utility methods for DateTime.
 * Author:   MikkoXU (mikkoxu@tvunetworks.com), added on Apr.20th, 2015.
 *           Originally in MyUtil.cs.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class DateTimeUtility
    {
        public static readonly DateTime DATETIME_1970 = new DateTime(1970, 1, 1);

        #region Unix timestamp to C# DateTime

        public static DateTime UnixSecondsToDateTime(long seconds)
        {
            return DATETIME_1970.AddSeconds(seconds);
        }

        public static DateTime UnixMillisecondsToDateTime(long milliseconds)
        {
            return DATETIME_1970.AddMilliseconds(milliseconds);
        }

        public static DateTime UnixTicksToDateTime(long ticks)
        {
            return DATETIME_1970.AddTicks(ticks);
        }

        #endregion

        #region C# DateTime to Unix timestamp

        public static ulong DateTimeToUnixSeconds(DateTime dt)
        {
            return (ulong)dt.Subtract(DATETIME_1970).TotalSeconds;
        }

        public static ulong DateTimeToUnixMilliseconds(DateTime dt)
        {
            return (ulong)dt.Subtract(DATETIME_1970).TotalMilliseconds;
        }

        public static ulong DateTimeToUnixTicks(DateTime dt)
        {
            return (ulong)dt.Subtract(DATETIME_1970).Ticks;
        }

        #endregion
    }
}
