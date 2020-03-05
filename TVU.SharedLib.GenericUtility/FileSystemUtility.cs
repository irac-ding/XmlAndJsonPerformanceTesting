/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: FileSystemUtility.cs
 * Purpose:  Utility methods for FileSystem.
 * Author:   Elizabeth added on Apr.18th, 2018.
 * Since:    Microsoft Visual Studio 2015 update3
 * =============================================*/

using System;
using System.IO;
using Mono.Unix;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class FileSystemUtility
    {
        public static void DeleteSubDirectories(string path)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    try
                    {
                        d.Delete(true);
                        LogManager.GetCurrentClassLogger().Warn($"DeleteSubDirectories() delete dir: {d.Name}.");
                    }
                    catch (Exception ex2)
                    {
                        LogManager.GetCurrentClassLogger().Error($"DeleteSubDirectories() inner error, {ex2.Message}.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"DeleteSubDirectories() error, {ex.Message}.");
            }
        }

        public static void DeleteDirAndSubDirectories(string path)
        {
            DeleteSubDirectories(path);
            try
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Delete(true);
                LogManager.GetCurrentClassLogger().Warn($"DeleteSubDirectories() delete dir: {di.Name}.");
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error($"DeleteDirAndSubDirectories() inner error, {ex.Message}.");
            }
        }

        public static void AddSymbolicLink(string linkDir, string sourceDir)
        {
            DirectoryInfo di = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, linkDir));
            if (di.Exists)
            {
                UnixSymbolicLinkInfo sli = new UnixSymbolicLinkInfo(di.FullName);
                if (!sli.IsSymbolicLink && sli.IsDirectory)
                {
                    DeleteSubDirectories(linkDir);
                    sli.CreateSymbolicLinkTo(sourceDir);
                    Console.WriteLine($"{linkDir} symbolic link has created.");
                }
                else if (sli.IsSymbolicLink)
                    Console.WriteLine($"{linkDir} symbolic link has created.");
            }
            else
            {
                UnixSymbolicLinkInfo sli = new UnixSymbolicLinkInfo(di.FullName);
                sli.CreateSymbolicLinkTo(sourceDir);
                Console.WriteLine($"{linkDir} symbolic link has created.");
            }
        }
    }
}
