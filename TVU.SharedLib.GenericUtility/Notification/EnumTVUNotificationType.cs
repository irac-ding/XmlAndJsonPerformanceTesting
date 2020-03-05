/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: EnumTVUNotificationType.cs
 * Purpose:  Enum of TVUNotification type.
 * Author:   MikkoXU added on July.23rd, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

namespace TVU.SharedLib.Notification
{
    public enum EnumTVUNotificationType : int
    {
        Unset = 0,
        Notification_Unknown = 100,
        Notification_Trace = 101,
        Notification_Debug = 102,
        Notification_Info = 103,
        Notification_Warn = 104,
        Notification_Error = 105,
        Notification_Fatal = 106,
    }
}
