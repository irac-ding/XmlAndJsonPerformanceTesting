/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only.
 * FileName: SRTEntity.cs
 * Purpose:  Using reflection to get properties of an instance of a class.
 * Author:   Amyyu created on July.27th, 2018.
 * Since:    Microsoft Visual Studio 2015 Update3
 * =============================================*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Web;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class ReflectionGetPropertiesUtility
    {
        #region Log

        private static Logger logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Method

        // Get all properties
        public static List<string> GetInstanceProperties(object model)
        {
            List<string> ret = null;
            try
            {
                var properties = model.GetType().GetProperties().Where(p => p.GetValue(model, null) != null && !p.GetValue(model, null).Equals((p.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute)?.Value)).Select(p => p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(model, null).ToString()));
                ret = properties.ToList();
                logger.Info("GetInstanceProperties() success.");
            }
            catch (Exception ex)
            {
                logger.Error($"GetInstanceProperties() failed: {ex.Message}");
            }

            return ret;
        }

        // Get non-inherited properties
        public static List<string> GetNonInheritedProperties(object model)
        {
            List<string> ret = null;
            try
            {
                var properties = model.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetValue(model, null) != null && !p.GetValue(model, null).Equals((p.GetCustomAttribute(typeof(DefaultValueAttribute)) as DefaultValueAttribute)?.Value)).Select(p => p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(model, null).ToString()));
                ret =properties.ToList();
                logger.Info($"GetNonInheritedProperties() success.");
            }
            catch (Exception ex)
            {
                logger.Error($"GetNonInheritedProperties() failed: {ex.Message}");
            }

            return ret;
        }

        #endregion
    }
}
