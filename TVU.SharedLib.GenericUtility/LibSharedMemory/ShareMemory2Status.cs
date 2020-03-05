/* =============================================
 * Copyright 2012 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only.
 * FileName: ShareMemory2Status.cs
 * Purpose:  Read ANSI content from ShareMemory.
 *           Refer to: https://msdn.microsoft.com/en-us/library/aa366791(v=vs.85).aspx.
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
using NLog;

namespace TVU.SharedLib.LibSharedMemory
{
    public class ShareMemory2Status
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields

        private readonly string _shareMemoryName = string.Empty;
        private readonly uint _viewOffset = 0;
        private readonly uint _viewSize = 1024;
        private readonly bool _isContentUnicode = false;

        private ShareMemoryPInvoke.SafeFileMappingHandle hMapFile = null;
        private IntPtr pView = IntPtr.Zero;

        private IntPtr pContentLength = IntPtr.Zero;
        private IntPtr pLastUpdate = IntPtr.Zero;
        private IntPtr pMessage = IntPtr.Zero;

        private ulong _lastUpdate = ulong.MinValue;

        private int _notUpdateCount = 0;
        private string _preReadToEnd = string.Empty;

        #endregion

        #region Constructors

        public ShareMemory2Status(string shareMemoryName)
        {
            _shareMemoryName = shareMemoryName;
        }

        public ShareMemory2Status(string shareMemoryName, uint viewOffset, uint viewSize, bool isContentUnicode)
            : this(shareMemoryName)
        {
            _viewOffset = viewOffset;
            _viewSize = viewSize;
            _isContentUnicode = isContentUnicode;
        }

        #endregion

        #region Methods

        private bool IsOpened()
        {
            return (hMapFile != null && pView != IntPtr.Zero);
        }

        public bool Open()
        {
            hMapFile = ShareMemoryPInvoke.NativeMethod.OpenFileMapping(ShareMemoryPInvoke.FileMapAccess.FILE_MAP_READ, false, _shareMemoryName);
            if (!hMapFile.IsInvalid)
            {
                pView = ShareMemoryPInvoke.NativeMethod.MapViewOfFile(hMapFile, ShareMemoryPInvoke.FileMapAccess.FILE_MAP_READ, 0, _viewOffset, _viewSize);
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

        public string ReadStatusFromShareMemory()
        {
            string ret = string.Empty;

            try
            {
                if (Open())
                {
                    if (CheckFlagAccessible())
                    {
                        ret = ReadFromShareMemory();
                    }
                    else
                    {
                        logger.Warn("ReadStatusFromShareMemory() flag is not accessible.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("ReadStatusFromShareMemory() error, {0}", ex.Message);
            }
            finally
            {
                Close();
            }

            return ret;
        }

        public string Read()
        {
            string ret = string.Empty;

            try
            {
                if (!IsOpened())
                {
                    logger.Info("Read() not open, open now.");
                    Open();
                }

                if (CheckFlagAccessible())
                {
                    ret = ReadFromShareMemory();
                }
                else
                {
                    logger.Warn("Read() flag is not accessible.");
                }
            }
            catch (Exception ex)
            {
                logger.Error("Read() error, {0}.", ex.Message);
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

            return ret;
        }

        private string ReadFromShareMemory()
        {
            if (IsOpened())
            {
                pContentLength = new IntPtr(pView.ToInt32() + 6);
                pLastUpdate = new IntPtr(pView.ToInt32() + 8);
                pMessage = new IntPtr(pView.ToInt32() + 21); // avoid the 0x3f BOM character in the head

                int length = Marshal.ReadInt16(pContentLength);
                logger.Debug("ReadFromShareMemory() raw length is {0}.", length);
                if (length > 2)
                {
                    length -= 2; // avoid the 0x3f at the beginning and the 0x0 ('\0') at the end.

                    ulong lastUpdate = (ulong)Marshal.ReadInt64(pLastUpdate);

                    if (lastUpdate - _lastUpdate > 0 || _lastUpdate == ulong.MinValue)
                    {
                        _notUpdateCount = 0;
                        _lastUpdate = lastUpdate;

                        string readToEnd = string.Empty;
                        if (_isContentUnicode)
                            readToEnd = Marshal.PtrToStringUni(pMessage, length);
                        else
                            readToEnd = Marshal.PtrToStringAnsi(pMessage, length);
                        logger.Debug("ReadFromShareMemory() get content {0}.", readToEnd);

                        _preReadToEnd = readToEnd;
                        return readToEnd;
                    }
                    else
                    {
                        _notUpdateCount++;
                        if (_notUpdateCount < 10)
                        {
                            return _preReadToEnd;
                        }
                        else
                        {
                            _preReadToEnd = string.Empty;
                        }
                    }
                }
            }

            return string.Empty;
        }

        #endregion
    }
}
