/* =============================================
 * Copyright 2013 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: GenericPInvoke.cs
 * Purpose:  PInvoke methods for generic purposes.
 * Author:   MikkoXU added on Nov.7th, 2014.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Runtime.InteropServices;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public enum CmdShow
    {
        SW_FORCEMINIMIZE = 11,
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
    }

    internal sealed class GenericPInvoke_Private
    {
        [DllImport("Ws2_32.dll")]
        internal static extern int WSACleanup();

        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }

    public sealed class GenericPInvoke
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static int WSACleanup()
        {
            try
            {
                return GenericPInvoke_Private.WSACleanup();
            }
            catch (Exception ex)
            {
                logger.Error("GenericPInvoke::WSACleanup() error {0}", ex.Message);
                return 0;
            }
        }

        public static bool ShowWindow(IntPtr hWnd, CmdShow cmd)
        {
            try
            {
                return GenericPInvoke_Private.ShowWindow(hWnd, (int)cmd);
            }
            catch (Exception ex)
            {
                logger.Error("GenericPInvoke::WSACleanup() error {0}", ex.Message);
                return false;
            }
        }

        public static bool SetForegroundWindow(IntPtr hWnd)
        {
            try
            {
                return GenericPInvoke_Private.SetForegroundWindow(hWnd);
            }
            catch (Exception ex)
            {
                logger.Error("GenericPInvoke::WSACleanup() error {0}", ex.Message);
                return false;
            }
        }
    }
}
