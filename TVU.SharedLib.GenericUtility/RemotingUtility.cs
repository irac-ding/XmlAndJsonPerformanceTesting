/* =============================================
 * Copyright 2015 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: RemotingUtility.cs
 * Purpose:  Utility methods for .Net Remoting.
 *           Always use WellKnownObjectMode.Singleton.
 * Author:   MikkoXU added on Nov.30th, 2015.
 * Since:    Microsoft Visual Studio 2008
 * =============================================*/

using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Channels.Tcp;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public sealed class RemotingUtility
    {
        #region Log

        private static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Server

        public static void CreateRemotingChannel<T>(IChannel channel, string objectName)
        {
            ChannelServices.RegisterChannel(channel, false);
            RemotingConfiguration.RegisterWellKnownServiceType(typeof(T), objectName, WellKnownObjectMode.Singleton);
        }

        public static void CreateIpcChannel<T>(string channelName, string objectName)
        {
            logger.Info("CreateIpcChannel() for {0} {1}.", channelName, objectName);

            IpcChannel ipcCh = new IpcChannel(channelName);
            CreateRemotingChannel<T>(ipcCh, objectName);
        }

        public static void CreateIpcChannel<T>(IDictionary properties, IServerChannelSinkProvider serverChannelSinkProvider, string objectName)
        {
            logger.Info("CreateIpcChannel() for {0}.", objectName);

            IpcChannel ipcCh = new IpcChannel(properties, null, serverChannelSinkProvider);
            CreateRemotingChannel<T>(ipcCh, objectName);
        }

        public static void CreateTcpChannel<T>(int port, string objectName)
        {
            logger.Info("CreateTcpChannel() for {0} {1}.", port, objectName);

            TcpChannel tcpCh = new TcpChannel(port);
            CreateRemotingChannel<T>(tcpCh, objectName);
        }

        public static void CreateHttpChannel<T>(int port, string objectName)
        {
            logger.Info("CreateHttpChannel() for {0} {1}.", port, objectName);

            HttpChannel httpCh = new HttpChannel(port);
            CreateRemotingChannel<T>(httpCh, objectName);
        }

        public static void CreateHttpChannel<T>(IDictionary properties, IServerChannelSinkProvider serverChannelSinkProvider, string objectName)
        {
            logger.Info("CreateHttpChannel() for {0}.", objectName);

            HttpChannel httpCh = new HttpChannel(properties, null, serverChannelSinkProvider);
            CreateRemotingChannel<T>(httpCh, objectName);
        }

        #endregion

        #region Client

        public static T ConnectToRemotingChannel<T>(IChannel channel, string url)
        {
            logger.Info("ConnectToRemotingChannel() url:{0}", url);

            ChannelServices.RegisterChannel(channel, false);
            T ret = (T)Activator.GetObject(typeof(T), url);
            return ret;
        }

        public static T ConnectToIpcChannel<T>(string channelName, string objectName, IDictionary properties)
        {
            logger.Info("ConnectToIpcChannel() for {0} {1}.", channelName, objectName);

            IpcChannel ipcCh = new IpcChannel(properties, null, null);
            string url = string.Format("ipc://{0}/{1}", channelName, objectName);
            return ConnectToRemotingChannel<T>(ipcCh, url);
        }

        public static T ConnectToTcpChannel<T>(string tcpIp, int tcpPort, string objectName, IDictionary properties)
        {
            logger.Info("ConnectToTcpChannel() for {0} {1} {2}.", tcpIp, tcpPort, objectName);

            TcpChannel tcpCh = new TcpChannel(properties, null, null);
            string url = string.Format("tcp://{0}:{1}/{2}", tcpIp, tcpPort, objectName);
            return ConnectToRemotingChannel<T>(tcpCh, url);
        }

        public static T ConnectToHttpChannel<T>(string httpIp, int httpPort, string objectName, IDictionary properties, SoapServerFormatterSinkProvider provider)
        {
            logger.Info("ConnectToTcpChannel() for {0} {1} {2}.", httpIp, httpPort, objectName);

            HttpChannel httpCh = new HttpChannel(properties, null, provider);
            string url = string.Format("http://{0}:{1}/{2}", httpIp, httpPort, objectName);
            return ConnectToRemotingChannel<T>(httpCh, url);
        }

        public static T ConnectToHttpChannel<T>(string httpIp, int httpPort, string objectName, ushort clientPort)
        {
            logger.Info("ConnectToHttpChannel() for {0} {1} {2} {3}.", httpIp, httpPort, objectName, clientPort);

            HttpChannel httpCh = new HttpChannel(clientPort);
            string url = string.Format("http://{0}:{1}/{2}", httpIp, httpPort, objectName);
            return ConnectToRemotingChannel<T>(httpCh, url);
        }

        #endregion
    }
}
