/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: EnumUtility.cs
 * Purpose:  Utility methods for Enum.
 * Author:   MikkoXU added on Nov.24th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;

namespace TVU.SharedLib.GenericUtility
{
    public static class EnumUtility
    {
        public static T ParseOrDefault<T>(object rawValue, T defaultValue)
        {
            T ret;

            if (Enum.IsDefined(typeof(T), rawValue))
            {
                ret = (T)Enum.Parse(typeof(T), rawValue.ToString(), true);
            }
            else
            {
                ret = defaultValue;
            }

            return ret;
        }

        public static bool TryParse<T>(object rawValue, ref T value)
        {
            bool ret = false;

            if (Enum.IsDefined(typeof(T), rawValue))
            {
                value = (T)Enum.Parse(typeof(T), rawValue.ToString(), true);
                ret = true;
            }

            return ret;
        }

        public static string GetDescription(this Enum value, Boolean nameInstead)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);

            if (name == null)
                return null;

            FieldInfo field = type.GetField(name);
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (attribute == null && nameInstead == true)
                return name;

            return attribute == null ? null : attribute.Description;
        }

        public static Dictionary<Int32, String> EnumToDictionary(Type enumType, Func<Enum, String> getText)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be enum.", "enumType");

            Dictionary<Int32, String> enumDic = new Dictionary<int, string>();
            Array enumValues = Enum.GetValues(enumType);

            foreach (Enum enumValue in enumValues)
            {
                Int32 key = Convert.ToInt32(enumValue);
                String value = getText(enumValue);
                enumDic.Add(key, value);
            }

            return enumDic;
        }

        public static object GetEnumValueBasedOnDescription(string description, Type enumType)
        {
            if (!enumType.IsEnum) throw new InvalidOperationException();
            foreach (var field in enumType.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", "description");
        }
    }
}
