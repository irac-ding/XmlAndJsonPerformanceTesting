/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: AbstractTVUInteractivity.cs
 * Purpose:  Base class for all kinds of interactivity.
 * Author:   MikkoXU added on Nov.10th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.ComponentModel;

namespace TVU.SharedLib.Notification
{
    public abstract class AbstractTVUInteractivity : INotifyPropertyChanged
    {
        #region Fields and properties

        protected static int _count = 0;
        public static int Count
        {
            get { return _count; }
        }

        public int Id { get; protected set; }

        public DateTime Timestamp { get; protected set; }

        #endregion

        #region Constructors

        protected AbstractTVUInteractivity()
        {
            Id = _count++;
            Timestamp = DateTime.Now;
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
