/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUPluginList.cs
 * Purpose:  Data class used to load plugin info from .json file.
 * Author:   MikkoXU added on July.27th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System.Collections.Generic;
using System.Linq;

namespace XmlAndJsonPerformanceTesting
{
    public class TVUPluginList
    {
        #region Properties

        public List<TVUPluginInfo> EnabledPlugins { get; set; }

        public List<TVUPluginInfo> DisabledPlugins { get; set; }

        #endregion

        #region Methods

        public List<TVUPluginInfo> MergePlugins()
        {
            List<TVUPluginInfo> ret = null;

            if (EnabledPlugins != null)
            {
                foreach (TVUPluginInfo pluginInfo in EnabledPlugins)
                    pluginInfo.Enabled = true;

                if (DisabledPlugins != null)
                    ret = EnabledPlugins.Union(DisabledPlugins).ToList();
                else
                    ret = EnabledPlugins;
            }
            else if (DisabledPlugins != null)
            {
                ret = DisabledPlugins;
            }

            return ret;
        }

        public void SyncEnableStatus(List<TVUPluginInfo> newList)
        {
            EnabledPlugins.Clear();
            DisabledPlugins.Clear();

            foreach (TVUPluginInfo plugInfo in newList)
            {
                if (plugInfo.Enabled)
                    EnabledPlugins.Add(plugInfo);
                else
                    DisabledPlugins.Add(plugInfo);
            }
        }

        #endregion
    }
}
