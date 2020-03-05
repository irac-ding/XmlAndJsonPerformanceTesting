/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: AttributeUtility.cs
 * Purpose:  Utility methods for Attribute.
 * Author:   MikkoXU added on Nov.24th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.ComponentModel;
using System.Reflection;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class AttributeUtility
    {
        #region For Assembly

        public static T GetAttribute<T>(Assembly obj) where T : Attribute
        {
            return (T)Attribute.GetCustomAttribute(obj, typeof(T), false);
        }

        #endregion

        #region For field

        public static T GetAttribute<T>(object obj) where T : Attribute
        {
            T ret = null;

            FieldInfo fi = obj.GetType().GetField(obj.ToString());
            T[] attributes = (T[])fi.GetCustomAttributes(typeof(T), false);

            if (attributes != null && attributes.Length > 0)
                ret = attributes[0];

            return ret;
        }

        public static string GetDescription(object obj)
        {
            DescriptionAttribute descriptionAttribute = GetAttribute<DescriptionAttribute>(obj);
            if (descriptionAttribute != null)
                return descriptionAttribute.Description;
            else
                return obj.ToString();
        }

        #endregion
    }
}
