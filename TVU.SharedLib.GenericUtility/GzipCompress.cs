/* =============================================
 * Copyright 2013 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: GzipCompress.cs
 * Purpose:  Compress message from R to TPDS by gzip.
 *           At first it uses ICSharpCode.SharpZipLib.GZip.GZipOutputStream,
 *           but it may related to RS-416 so it is replaced by
 *           System.IO.Compression.GZipStream on June.23rd, 2014.
 *           The compression rate is not as high as before (about 25% lower).
 * Author:   MikkoXU (mikkoxu@tvunetworks.com)
 * History:  MikkoXU changed the dependency library on June.23rd, 2014.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System.IO;
using System.IO.Compression;
using System.Text;

namespace TVU.SharedLib.GenericUtility
{
    public class GzipCompress
    {
        private static byte[] Compress(byte[] bytesToCompress)
        {
            byte[] toReturn = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (var s = new GZipStream(ms, CompressionMode.Compress))
                {
                    s.Write(bytesToCompress, 0, bytesToCompress.Length);
                }
                toReturn = ms.ToArray();
            }
            return toReturn;
        }

        public static byte[] CompressToByte(string stringToCompress)
        {
            byte[] toCompress = Encoding.Default.GetBytes(stringToCompress);
            return Compress(toCompress);
        }
    }
}
