/* =============================================
 * Copyright 2016 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: Base64ImageUtility.cs
 * Purpose:  Utility methods for base64 to image and image to base64.
 * Author:   ElizabethHe added on Nov.22th, 2016.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class Base64ImageUtility
    {
        public static string ImageToBase64(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        public static Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }

        public static string GetFilenameExtension(ImageFormat format)
        {
            return ImageCodecInfo.GetImageEncoders().FirstOrDefault(x => x.FormatID == format.Guid).FilenameExtension.ToLower();
        }

        public static string GetFilenameExtensionWithoutWildcard(ImageFormat format)
        {
            return GetFilenameExtension(format).Replace(@"*.", string.Empty).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).First().ToLower();
        }
    }
}
