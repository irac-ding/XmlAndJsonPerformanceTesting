/* =============================================
 * Copyright 2012 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only.
 * FileName: Status2ShareMemory.cs
 * Purpose:  Write ANSI content to ShareMemory.
 *           Refer to: https://msdn.microsoft.com/en-us/library/windows/desktop/aa366537(v=vs.85).aspx.
 *           It was used in Grid status transfer but actually it can transfer anything,
 *           just requires some certain fiels in the first 21 bytes:
 *           0~3 lock,
 *           4~5 version, 6~7 content_length,
 *           8~15 last_update_timecode (in microseconds),
 *           16~19 reserved_area,
 *           20~end content (with 0x3f BOM character in the head).
 * Author:   MikkoXU added on Jan.11th, 2013.
 * History:  MikkoXU simplified the logic on March.19th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Runtime.InteropServices;
using System.Text;
using NLog;
using TVU.SharedLib.GenericUtility;

namespace TVU.SharedLib.LibSharedMemory
{
    public class Status2ShareMemory
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private readonly string _shareMemoryName = string.Empty;
        private readonly uint _mapSize = 65536;
        private readonly uint _viewOffset = 0;
        private readonly uint _viewSize = 1024;
        private readonly bool _isContentUnicode = false;
        private readonly byte[] _bVersion = BitConverter.GetBytes((ushort)1);

        private ShareMemoryPInvoke.SafeFileMappingHandle hMapFile = null;
        private IntPtr pView = IntPtr.Zero;

        private IntPtr pVersion = IntPtr.Zero;
        private IntPtr pContentLength = IntPtr.Zero;
        private IntPtr pLastUpdate = IntPtr.Zero;
        private IntPtr pReservedArea = IntPtr.Zero;
        private IntPtr pMessage = IntPtr.Zero;

        #endregion

        #region Constructors

        public Status2ShareMemory(string shareMemoryName)
        {
            _shareMemoryName = shareMemoryName;
        }

        public Status2ShareMemory(string shareMemoryName, uint mapSize, uint viewOffset, uint viewSize, bool isContentUnicode, uint version)
            : this(shareMemoryName)
        {
            _mapSize = mapSize;
            _viewOffset = viewOffset;
            _viewSize = viewSize;
            _isContentUnicode = isContentUnicode;
            _bVersion = BitConverter.GetBytes((ushort)version);
        }

        #endregion

        #region Methods

        private bool IsOpened()
        {
            return (hMapFile != null && pView != IntPtr.Zero);
        }

        private bool Open()
        {
            hMapFile = ShareMemoryPInvoke.NativeMethod.CreateFileMapping(ShareMemoryPInvoke.INVALID_HANDLE_VALUE, IntPtr.Zero, ShareMemoryPInvoke.FileProtection.PAGE_READWRITE, 0, _mapSize, _shareMemoryName);
            if (!hMapFile.IsInvalid)
            {
                pView = ShareMemoryPInvoke.NativeMethod.MapViewOfFile(hMapFile, ShareMemoryPInvoke.FileMapAccess.FILE_MAP_ALL_ACCESS, 0, _viewOffset, _viewSize);
                logger.Trace("Open() get pView {0}.", pView);
                if (pView != IntPtr.Zero)
                {
                    return true;
                }
            }

            hMapFile = null;
            pView = IntPtr.Zero;
            return false;
        }

        public void Close()
        {
            if (hMapFile != null)
            {
                if (pView != IntPtr.Zero)
                {
                    ShareMemoryPInvoke.NativeMethod.UnmapViewOfFile(pView);
                    pView = IntPtr.Zero;
                }

                hMapFile.Close();
                hMapFile = null;
            }
        }

        public bool WriteStatus2ShareMemory(string message)
        {
            bool ret = false;

            try
            {
                if (!IsOpened())
                    Open();

                if (CheckFlagAccessible())
                {
                    SetFlag(1);

                    Write2ShareMemory(message);

                    SetFlag(0);

                    ret = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error("WriteStatus2ShareMemory() error, {0}.", ex.Message);
                SetFlag(0);
                Close();
            }

            return ret;
        }

        private bool CheckFlagAccessible()
        {
            bool ret = false;

            if (IsOpened())
            {
                int flag = Marshal.ReadInt32(pView);
                ret = (flag == 0);
            }
            else
                logger.Warn("CheckFlagAccessible() failed, IsOpened is false.");

            return ret;
        }

        private void SetFlag(int flag)
        {
            if (IsOpened())
            {
                Marshal.WriteInt32(pView, flag);
            }
        }

        private void Write2ShareMemory(string message)
        {
            if (IsOpened())
            {
                byte[] bMessage = null;
                if (_isContentUnicode)
                    bMessage = Encoding.Unicode.GetBytes(message + '\0');
                else
                    bMessage = Encoding.ASCII.GetBytes(message + '\0');

                byte[] bContent_length = BitConverter.GetBytes(bMessage.Length);
                byte[] bLast_update = BitConverter.GetBytes(DateTimeUtility.DateTimeToUnixTicks(DateTime.UtcNow));
                byte[] bReserved_area = BitConverter.GetBytes(0);

                if (pVersion == IntPtr.Zero)
                    pVersion = new IntPtr(pView.ToInt32() + 4);
                if (pContentLength == IntPtr.Zero)
                    pContentLength = new IntPtr(pView.ToInt32() + 6);
                if (pLastUpdate == IntPtr.Zero)
                    pLastUpdate = new IntPtr(pView.ToInt32() + 8);
                if (pReservedArea == IntPtr.Zero)
                    pReservedArea = new IntPtr(pView.ToInt32() + 16);
                if (pMessage == IntPtr.Zero)
                    pMessage = new IntPtr(pView.ToInt32() + 20);

                Marshal.Copy(_bVersion, 0, pVersion, _bVersion.Length);
                Marshal.Copy(bContent_length, 0, pContentLength, bContent_length.Length);
                Marshal.Copy(bLast_update, 0, pLastUpdate, bLast_update.Length);
                Marshal.Copy(bReserved_area, 0, pReservedArea, bReserved_area.Length);
                Marshal.Copy(bMessage, 0, pMessage, bMessage.Length);
            }
        }

        #endregion
    }
}
