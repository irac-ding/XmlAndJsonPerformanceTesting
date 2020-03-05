/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUPluginInfo.cs
 * Purpose:  Description info for TVUPlugin.
 * Author:   MikkoXU added on July.27th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System.ComponentModel;
using Newtonsoft.Json;

namespace XmlAndJsonPerformanceTesting
{
    public class TVUPluginInfo : INotifyPropertyChanged
    {
        public string DllPath { get; set; }

        private bool _enabled = false;
        [JsonIgnore]
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    Notify("Enabled");
                }
            }
        }

        public TVUPluginInfo()
        {
        }

        public TVUPluginInfo(string dllPath, bool enabled)
        {
            DllPath = dllPath;
            Enabled = enabled;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

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
