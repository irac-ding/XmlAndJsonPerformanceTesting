/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: PerformanceReport.cs
 * Purpose:  Performance report sent to NORAD Server.
 * Author:   MikkoXU added on July.9th, 2015.
 * History:  EllaLiu copied from TVU.NORAD.Reporter.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using Newtonsoft.Json;
using NLog;
using System;
using System.Diagnostics;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class PerformanceUtility 
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Public Methods

        public static PerformanceInfo GetPerformanceInfo()
        {
            PerformanceInfo result = null;

            try
            {
                long privateMemorySize64 = Process.GetCurrentProcess().PrivateMemorySize64;
                int threads = Process.GetCurrentProcess().Threads.Count;
                int handles = Process.GetCurrentProcess().HandleCount;
                ulong startTimestamp = DateTimeUtility.DateTimeToUnixMilliseconds(Process.GetCurrentProcess().StartTime.ToUniversalTime());
                double totalProcessorTime = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds;

                logger.Debug("GetPerformanceInfo() private memory size64:{0} threads:{1} handles:{2} start timestamp:{3} total processor time:{4}", privateMemorySize64, threads, handles, startTimestamp, totalProcessorTime);

                result = new PerformanceInfo(privateMemorySize64, threads, handles, startTimestamp, totalProcessorTime);
            }
            catch(Exception ex)
            {
                logger.Error("GetPerformanceInfo() error: {0}", ex.Message);
                result = null;
            }

            return result;
        }

        #endregion
    }

    public class PerformanceInfo
    {
        #region Properties

        public long PrivateMemorySize64 { get; private set; }

        public int Threads { get; private set; }

        public int Handles { get; private set; }

        public ulong StartTime { get; private set; }

        public double TotalProcessorTime { get; private set; }

        #endregion

        #region Constructors

        public PerformanceInfo(long privateMemorySize64, int threads, int handles, ulong startTimestamp, double totalProcessTime)
        {
            PrivateMemorySize64 = privateMemorySize64;
            Threads = threads;
            Handles = handles;
            StartTime = startTimestamp;
            TotalProcessorTime = totalProcessTime;
        }

        #endregion

        #region Json

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        #endregion
    }
}
