/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUNotification.cs
 * Purpose:  Data class including necessary information of a notification.
 * Author:   MikkoXU added on July.23rd, 2015.
 * Ref:      http://www.codeproject.com/Articles/499241/Growl-Alike-WPF-Notifications.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.ComponentModel;

namespace TVU.SharedLib.Notification
{
    public class TVUNotification : INotifyPropertyChanged
    {
        #region Fields and properties

        private static int _count = 1;
        public static int Count
        {
            get { return _count; }
        }

        public int Id { get; private set; }

        public DateTime Timestamp { get; private set; }

        public EnumTVUNotificationType Type { get; private set; }

        public string Title { get; private set; }

        public string Message { get; private set; }

        private string _iconUrl = string.Empty;
        public string IconUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_iconUrl))
                    return _iconUrl;
                else
                {
                    switch (Type)
                    {
                        case EnumTVUNotificationType.Notification_Trace:
                        case EnumTVUNotificationType.Notification_Debug:
                        case EnumTVUNotificationType.Notification_Info:
                            return "pack://application:,,,/Common;component/Images/Info.png";
                        case EnumTVUNotificationType.Notification_Warn:
                        case EnumTVUNotificationType.Notification_Error:
                        case EnumTVUNotificationType.Notification_Fatal:
                            return "pack://application:,,,/Common;component/Images/Error.png";
                        case EnumTVUNotificationType.Notification_Unknown:
                        default:
                            return "pack://application:,,,/Common;component/Images/Info.png";
                    }
                }
            }
            set
            {
                _iconUrl = value;
                Notify("IconUrl");
            }
        }

        #endregion

        #region Constructors

        public TVUNotification()
        {
            Id = _count++;
            Timestamp = DateTime.Now;
        }

        public TVUNotification(EnumTVUNotificationType type, string title, string message, string iconUrl)
            : this()
        {
            Type = type;
            Title = title;
            Message = message;
            IconUrl = iconUrl;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void Notify(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
