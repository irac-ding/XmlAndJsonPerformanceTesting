/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: NotificationCenterModel.cs
 * Purpose:  Model of Notification Center.
 * Author:   MikkoXU added on July.23rd, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using NLog;

namespace TVU.SharedLib.Notification
{
    public class NotificationCenterModel
    {
        #region Log

        private Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Singleton

        private static object _lockSyncRoot = new object();

        private static volatile NotificationCenterModel _instance = null;
        public static NotificationCenterModel Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockSyncRoot)
                    {
                        if (_instance == null)
                            _instance = new NotificationCenterModel();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Delegates

        public Action<TVUNotification> NotificationAdded { get; set; }
        public Action<TVUInteractivity> InteractivityAdded { get; set; }

        #endregion

        #region Properties

        public int MaxNotifications { get; private set; }

        public int NotificationItemHeight { get; private set; }

        public int NotificationItemMargin { get; private set; }

        public int WndHeight { get; private set; }

        #endregion

        #region Constructors

        private NotificationCenterModel()
        {
            MaxNotifications = 4;
            NotificationItemHeight = 120;
            NotificationItemMargin = 10;
            WndHeight = MaxNotifications * (NotificationItemHeight + NotificationItemMargin * 2 + 10);
        }

        #endregion

        #region Methods

        public void AddNotification(TVUNotification notification)
        {
            if (NotificationAdded != null)
                NotificationAdded(notification);
        }

        public void AddInteractivity(TVUInteractivity interactivity)
        {
            if (InteractivityAdded != null)
                InteractivityAdded(interactivity);
        }

        #endregion
    }
}
