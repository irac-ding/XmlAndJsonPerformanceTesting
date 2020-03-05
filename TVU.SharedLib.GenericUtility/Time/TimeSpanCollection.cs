/* =============================================
 * Copyright 2014 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TimeSpanCollection.cs
 * Purpose:  Data class contains a range of year, month, day, hour, minute and second.
 * Author:   VickyDuan added on July.4th, 2014.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System.Collections.Generic;

namespace TVU.SharedLib.GenericUtility
{
    public class TimeSpanCollection
    {
        private List<int> _listYear = new List<int>();
        private List<int> _listMonth = new List<int>();
        private List<int> _listDay = new List<int>();
        private List<int> _listHour = new List<int>();
        private List<int> _listMinute = new List<int>();
        private List<int> _listSecond = new List<int>();

        public TimeSpanCollection()
        {
            for (int i = 0; i < 100; i++)
                _listYear.Add(i);
            for (int i = 0; i < 12; i++)
                _listMonth.Add(i);
            for (int i = 0; i < 31; i++)
                _listDay.Add(i);
            for (int i = 0; i < 24; i++)
                _listHour.Add(i);
            for (int i = 0; i < 60; i++)
                _listMinute.Add(i);
            for (int i = 0; i < 60; i++)
                _listSecond.Add(i);
        }

        public List<int> ListYear
        {
            get { return _listYear; }
        }

        public List<int> ListMonth
        {
            get { return _listMonth; }
        }

        public List<int> ListDay
        {
            get { return _listDay; }
        }

        public List<int> ListHour
        {
            get { return _listHour; }
        }

        public List<int> ListMinute
        {
            get { return _listMinute; }
        }

        public List<int> ListSecond
        {
            get { return _listSecond; }
        }
    }
}
