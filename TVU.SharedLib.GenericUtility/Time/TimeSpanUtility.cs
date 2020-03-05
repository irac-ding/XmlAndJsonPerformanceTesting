/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TimeSpanUtility.cs
 * Purpose:  Utility methods for TimeSpan.
 * Author:   MikkoXU (mikkoxu@tvunetworks.com), added on Apr.20th, 2015.
 * Ref-to:   https://stackoverflow.com/questions/16689468/how-to-produce-human-readable-strings-to-represent-a-timespan/21649465 .
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class TimeSpanUtility
    {
        #region Regular to C# TimeSpan

        public static TimeSpan SecondsToTimeSpan(long seconds)
        {
            return new TimeSpan(seconds * 10000000);
        }

        public static TimeSpan MillisecondsToTimeSpan(long milliseconds)
        {
            return new TimeSpan(milliseconds * 10000);
        }

        #endregion

        #region C# TimeSpan to regular

        public static long TimeSpanToSeconds(TimeSpan ts)
        {
            return (long)ts.TotalSeconds;
        }

        public static long TimeSpanToMilliseconds(TimeSpan ts)
        {
            return (long)ts.TotalMilliseconds;
        }

        #endregion

        #region C# TimeSpan to readable string

        public static string GetReadableTimespan(TimeSpan ts)
        {
            SortedList<long, string> cutoff = new SortedList<long, string> {
                { 60 - 1, "{3:S}" },
                { 60 * 60 - 1, "{2:M}, {3:S}" },
                { 24 * 60 * 60 - 1, "{1:H}, {2:M}" },
                { long.MaxValue, "{0:D}, {1:H}" }
            };

            int find = cutoff.Keys.ToList().BinarySearch((long)ts.TotalSeconds);
            int near = find < 0 ? Math.Abs(find) - 1 : find;
            return string.Format(
                new HMSFormatter(),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds);
        }

        #endregion
    }

    public class HMSFormatter : ICustomFormatter, IFormatProvider
    {
        static Dictionary<string, string> timeformats = new Dictionary<string, string> {
            { "S", "{0:P:Seconds:Second}" },
            { "M", "{0:P:Minutes:Minute}" },
            { "H", "{0:P:Hours:Hour}" },
            { "D", "{0:P:Days:Day}" }
        };

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return string.Format(new PluralFormatter(), timeformats[format], arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }

    public class PluralFormatter : ICustomFormatter, IFormatProvider
    {
        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg != null)
            {
                string[] parts = format.Split(':');
                if (parts[0] == "P")
                {
                    int partIndex = (arg.ToString() == "1") ? 2 : 1;
                    return string.Format("{0} {1}", arg, (parts.Length > partIndex ? parts[partIndex] : ""));
                }
            }
            return string.Format(format, arg);
        }

        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }
    }
}
