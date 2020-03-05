/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUInteractivity.cs
 * Purpose:  Data class including necessary information of an interactivity.
 * Author:   MikkoXU added on Oct.15th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;

namespace TVU.SharedLib.Notification
{
    public class TVUInteractivity : AbstractTVUInteractivity
    {
        #region Fields and properties

        public EnumTVUInteractivityType Type { get; private set; }

        public string Title { get; private set; }

        public string Message { get; private set; }

        public Action OnYes { get; private set; }

        public Action OnNo { get; private set; }

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
                        case EnumTVUInteractivityType.Interactivity_YesNoQuestion:
                            return "pack://application:,,,/Common;component/Images/Question.png";
                        case EnumTVUInteractivityType.Interactivity_Unknown:
                        default:
                            return "pack://application:,,,/Common;component/Images/Question.png";
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

        public TVUInteractivity(EnumTVUInteractivityType type, string title, string message, Action onYes, Action onNo, string iconUrl)
            : base()
        {
            Type = type;
            Title = title;
            Message = message;
            OnYes = onYes;
            OnNo = onNo;
            IconUrl = iconUrl;
        }

        #endregion
    }
}
