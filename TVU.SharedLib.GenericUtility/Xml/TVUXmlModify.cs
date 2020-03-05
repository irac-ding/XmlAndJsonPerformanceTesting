/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved
 * For internal members in TVU Networks only
 * FileName: TVUXmlModify.cs
 * Purpose:  Provide method to modify XML.
 * Author:   AriZheng added on Jul.3th, 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Xml;
using NLog;
using TVU.SharedLib.GenericUtility.FileRescue;

namespace TVU.SharedLib.Xml
{
    public sealed class TVUXmlModify
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        public static bool ChangeNodeValue(string nodePath, string newValue, string filePath, bool needRescue = true)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc = RescueFileOperation.RescueRead(filePath, Read, needRescue);
                XmlNode node = xmlDoc.SelectSingleNode(nodePath);
                if (node != null)
                {
                    node.Value = newValue;
                    RescueFileOperation.RescueSave(xmlDoc, filePath, Write, needRescue);
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChangeNodeValue() error, error mesage: {0}", ex.Message);
                return false;
            }
            return true;
        }

        public static bool ChangeAttributeValue(string nodePath, string attributeName, string newValue, string filePath, bool needRescue = true)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc = RescueFileOperation.RescueRead(filePath, Read, needRescue);
                XmlNode node = xmlDoc.SelectSingleNode(nodePath);
                if (node != null)
                {
                    node.Attributes[attributeName].Value = newValue;
                    RescueFileOperation.RescueSave(xmlDoc, filePath, Write, needRescue);
                }
            }
            catch (Exception ex)
            {
                logger.Error("ChangeAttributeValue() error, error mesage: {0}", ex.Message);
                return false;
            }
            return true;
        }

        public static bool RemoveSpecialAttribute(string nodePath, string attributeName, string filePath, bool needRescue = true)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc = RescueFileOperation.RescueRead(filePath, Read, needRescue);
                XmlNode node = xmlDoc.SelectSingleNode(nodePath);
                if (node != null)
                {
                    node.Attributes.Remove(node.Attributes[attributeName]);
                    RescueFileOperation.RescueSave(xmlDoc, filePath, Write, needRescue);
                }
            }
            catch (Exception ex)
            {
                logger.Error("RemoveSpecialAttribute() error, error mesage: {0}", ex.Message);
                return false;
            }
            return true;
        }

        public static bool RemoveAllAttributes(string nodePath, string filePath, bool needRescue = true)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc = RescueFileOperation.RescueRead(filePath, Read, needRescue);
                XmlNode node = xmlDoc.SelectSingleNode(nodePath);
                if (node != null)
                {
                    node.Attributes.RemoveAll();
                    RescueFileOperation.RescueSave(xmlDoc, filePath, Write, needRescue);
                }
            }
            catch (Exception ex)
            {
                logger.Error("RemoveAllAttributes() error, error mesage: {0}", ex.Message);
                return false;
            }
            return true;
        }

        #region Read & Write

        private static XmlDocument Read(string configFile)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFile);
            return xmlDoc;
        }

        private static void Write(object xmlDoc, string configFile)
        {
            ((XmlDocument)xmlDoc).Save(configFile);
        }

        #endregion Read & Write
    }
}
