/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUAssemblyInfo.cs
 * Purpose:  Data class stores necessary information from AssemblyInfo.cs.
 * Author:   MikkoXU added on Nov.24th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Reflection;
using Newtonsoft.Json;

namespace TVU.SharedLib.GenericUtility
{
    public class TVUAssemblyInfo
    {
        #region Properties

        public string Title { get; set; }

        public string Description { get; set; }

        public string Configuration { get; set; }

        public string Company { get; set; }

        public string Product { get; set; }

        public string Copyright { get; set; }

        public string Trademark { get; set; }

        public Version AssemblyVersion { get; set; }

        public string AssemblyFileVersion { get; set; }

        #endregion

        #region Constructors

        public TVUAssemblyInfo()
        {
        }

        public TVUAssemblyInfo(string title, string description, string configuration, string company, string product, string copyright, string trademark, Version assemblyVersion, string assemblyFileVersion)
        {
            Title = title;
            Description = description;
            Configuration = configuration;
            Company = company;
            Product = product;
            Copyright = copyright;
            Trademark = trademark;
            AssemblyVersion = assemblyVersion;
            AssemblyFileVersion = assemblyFileVersion;
        }

        #endregion

        #region Helper methods

        public static TVUAssemblyInfo GetTVUAssemblyInfo(Assembly assembly)
        {
            try
            {
                string title = AttributeUtility.GetAttribute<AssemblyTitleAttribute>(assembly).Title;
                string description = AttributeUtility.GetAttribute<AssemblyDescriptionAttribute>(assembly).Description;
                string configuration = AttributeUtility.GetAttribute<AssemblyConfigurationAttribute>(assembly).Configuration;
                string company = AttributeUtility.GetAttribute<AssemblyCompanyAttribute>(assembly).Company;
                string product = AttributeUtility.GetAttribute<AssemblyProductAttribute>(assembly).Product;
                string copyright = AttributeUtility.GetAttribute<AssemblyCopyrightAttribute>(assembly).Copyright;
                string trademark = AttributeUtility.GetAttribute<AssemblyTrademarkAttribute>(assembly).Trademark;
                Version assemblyVersion = assembly.GetName().Version;
                string assemblyFileVersion = AttributeUtility.GetAttribute<AssemblyFileVersionAttribute>(assembly).Version;

                return new TVUAssemblyInfo(title, description, configuration, company, product, copyright, trademark, assemblyVersion, assemblyFileVersion);
            }
            catch
            {
                return new TVUAssemblyInfo();
            }
        }

        #endregion

        #region Json

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion
    }
}
