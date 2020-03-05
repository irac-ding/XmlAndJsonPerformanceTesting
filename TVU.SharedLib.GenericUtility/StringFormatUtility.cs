/* =============================================
 * Copyright 2016 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: StringFormatUtility.cs
 * Purpose:  String Extension for replace the target string by Regex.
 * Author:   ElizabethHe added on feb.23th, 2016.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TVU.SharedLib.GenericUtility
{
    public static class StringFormatUtility
    {
        static readonly Regex DefaultRegex = new Regex(@"\{([a-zA-Z]+)\}", RegexOptions.Compiled);

        public static string FormatPlaceholder(this string str, Regex userRegex, int groupNumber, Dictionary<string, string> fields)
        {
            if (fields == null)
                return str;

            //return userRegex.Replace(str, match => fields[match.Groups[1].Value]);
            return userRegex.Replace(str, match => fields.ContainsKey(match.Groups[groupNumber].Value) ? fields[match.Groups[groupNumber].Value] : match.ToString());
        }

        public static string FormatPlaceholder(this string str, Dictionary<string, string> fields)
        {
            return str.FormatPlaceholder(DefaultRegex, 1, fields);
        }
    }
}
