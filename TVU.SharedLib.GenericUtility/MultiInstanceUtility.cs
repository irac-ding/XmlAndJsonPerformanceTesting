/* =============================================
 * Copyright 2017 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: MultiInstanceUtility.cs
 * Purpose:  Handle multi instance path such as ##1## and ##2##.
 * Author:   MikkoXU added on Jan.22nd, 2017.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System.Text.RegularExpressions;

namespace TVU.SharedLib.GenericUtility
{
    public class MultiInstanceUtility
    {
        public static readonly string MultipleInstancePostfix = @"##\";
        public static readonly string MultipleInstancePattern = @".*##(\d*)##\\";

        public static int ParseInstanceIndex(string path)
        {
            int ret = -1;

            if (path.EndsWith(MultipleInstancePostfix))
            {
                Match match = null;
                if (null != (match = Regex.Match(path, MultipleInstancePattern)))
                {
                    if (match.Groups.Count >= 2)
                    {
                        string strInstanceIndex = match.Groups[1].ToString();
                        int instanceIndex;
                        if (int.TryParse(strInstanceIndex, out instanceIndex))
                        {
                            ret = instanceIndex;
                        }
                    }
                }
            }

            return ret;
        }
    }
}
