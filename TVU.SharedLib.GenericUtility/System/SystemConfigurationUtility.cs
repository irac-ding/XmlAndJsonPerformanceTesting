/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: SystemConfigurationUtility.cs
 * Purpose:  System configuration report sent to Sentry.
 * Author:   MikkoXU added on July.9th, 2015.
 * History:  EllaLiu copied from TVU.NORAD.Reporter.SystemConfigurationReport.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using Newtonsoft.Json;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class SystemConfigurationUtility
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static SystemConfigurationInfo GetSystemConfigurationInfo()
        {
            SystemConfigurationInfo result = null;

            try
            {
                string osCaption = GetManagementObjectInfo("Win32_OperatingSystem", "Caption");
                string osArchitecture = GetManagementObjectInfo("Win32_OperatingSystem", "OSArchitecture");

                string systemManufacturer = GetManagementObjectInfo("Win32_ComputerSystem", "Manufacturer");
                string systemModel = GetManagementObjectInfo("Win32_ComputerSystem", "Model");
                string memory = GetManagementObjectInfo("Win32_ComputerSystem", "TotalPhysicalMemory");

                string motherboardName = GetManagementObjectInfo("Win32_BaseBoard", "Product");

                string hddName = GetManagementObjectInfo("Win32_DiskDrive", "Model");

                string biOSVersion = GetManagementObjectInfo("Win32_BIOS", "Caption");

                string processor = GetManagementObjectInfo("Win32_Processor", "Name");

                List<string> listDisplayAdapters = GetManagementObjectInfoList("Win32_VideoController", "Description");

                List<string> listDisplayAdapterDrivers = GetManagementObjectInfoList("Win32_VideoController", "DriverVersion");

                result = new SystemConfigurationInfo(osCaption, osArchitecture, systemManufacturer, systemModel, memory, motherboardName, hddName, biOSVersion, processor, listDisplayAdapters, listDisplayAdapterDrivers);
            }
            catch (Exception ex)
            {
                logger.Error("GetSystemConfigurationInfo() error: {0}", ex.Message);
                result = null;
            }

            return result;
        }

        #region Private Methods

        private static string GetManagementObjectInfo(string strFrom, string strWmi)
        {
            string ret = string.Empty;

            try
            {
                using (ManagementObjectSearcher search = new ManagementObjectSearcher(string.Format("SELECT * FROM {0}", strFrom)))
                {
                    foreach (ManagementObject wmi in search.Get())
                    {
                        ret = wmi[strWmi].ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetManagementObjectInfo() for strFrom {0} strWmi {1} error, {0}.", strFrom, strWmi, ex.Message);
            }

            return ret;
        }

        private static List<string> GetManagementObjectInfoList(string strFrom, string strWmi)
        {
            List<string> ret = new List<string>();

            try
            {
                using (ManagementObjectSearcher search = new ManagementObjectSearcher(string.Format("SELECT * FROM {0}", strFrom)))
                {
                    foreach (ManagementObject wmi in search.Get())
                    {
                        ret.Add(wmi[strWmi].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("GetManagementObjectInfoList() for strFrom {0} strWmi {1} error, {0}.", strFrom, strWmi, ex.Message);
            }

            return ret;
        }

        #endregion
    }

    public class SystemConfigurationInfo
    {
        #region Static properties

        public static bool IsWindows { get; private set; }

        #endregion

        #region Properties

        public string ComputerName { get; private set; }

        public PlatformID OSPlatform { get; private set; }

        public string OSVersion { get; private set; }

        public string OSCaption { get; private set; }

        public string OSArchitecture { get; private set; }

        public string Language { get; private set; }

        public string SystemManufacturer { get; private set; }

        public string SystemModel { get; private set; }

        public string MotherboardName { get; private set; }

        public string HDDName { get; private set; }

        public string BIOSVersion { get; private set; }

        public string Processor { get; private set; }

        public string Memory { get; private set; }

        public List<string> ListDisplayAdapters { get; private set; }

        public List<string> ListDisplayAdapterDrivers { get; private set; }

        #endregion

        #region Constructors

        static SystemConfigurationInfo()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            IsWindows = (platform != PlatformID.Unix && platform != PlatformID.MacOSX);
        }

        public SystemConfigurationInfo(string osCaption, string osArchitecture, string systemManufacturer, string systemModel, string memory, string motherboardName, string hddName, string biOSVersion, string processor, List<string> listDisplayAdapters, List<string> listDisplayAdapterDrivers)
        {
            ComputerName = Environment.MachineName;
            OSPlatform = Environment.OSVersion.Platform;
            OSVersion = Environment.OSVersion.ToString();
            Language = CultureInfo.CurrentCulture.EnglishName;

            OSCaption = osCaption;
            OSArchitecture = osArchitecture;

            SystemManufacturer = systemManufacturer;
            SystemModel = systemModel;
            Memory = memory;

            MotherboardName = motherboardName;

            HDDName = hddName;

            BIOSVersion = biOSVersion;
            Processor = processor;
            ListDisplayAdapters = listDisplayAdapters;
            ListDisplayAdapterDrivers = listDisplayAdapterDrivers;
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
