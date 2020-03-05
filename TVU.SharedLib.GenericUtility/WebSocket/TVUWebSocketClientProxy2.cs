/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUWebSocketClientProxy2.cs
 * Purpose:  Manage all the outbound websocket connections.
 * Author:   EllaLiu(ellaliu@tvunetworks.com), added on Jun.13th, 2016.
 *           MikkoXU made a copy to integrate with https://github.com/sta/websocket-sharp on Mar.19th, 2018.
 *           No status check thread for now. 
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace TVU.SharedLib.GenericUtility
{
    public class TVUWebSocketClientProxy2 : IWebSocketClientProxy
    {
        #region Log

        private Logger logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields And Properties

        private const int DEFAULT_TIMEOUT_MS = 1000;

        private static readonly object _lockDictClients = new object();
        private Dictionary<string, TVUWebSocketClient2> _dictClients { get; } = new Dictionary<string, TVUWebSocketClient2>();

        #endregion

        #region Public Methods

        public void RegisterClient(string clientId, string serverUrl, GetInitMsgHandler getInitMsg, MessageReceivedHandler onMsgReceived, GetExitMsgHandler getExitMsg, bool isCheckStatus = false, int checkStatusInv = 2, int timeoutSeconds = 20)
        {
            logger.Info("RegisterClient() clientId:{0} server url:{1}", clientId, serverUrl);

            if (!_dictClients.ContainsKey(clientId))
            {
                if (Monitor.TryEnter(_lockDictClients, DEFAULT_TIMEOUT_MS))
                {
                    try
                    {
                        if (!_dictClients.ContainsKey(clientId))
                        {
                            TVUWebSocketClient2 client = new TVUWebSocketClient2(clientId, serverUrl);

                            if (getInitMsg != null)
                                client.GetInitMessage += getInitMsg;

                            if (getExitMsg != null)
                                client.GetExitMessage += getExitMsg;

                            if (onMsgReceived != null)
                                client.MessageReceived += onMsgReceived;

                            _dictClients.Add(clientId, client);
                            client.Connect();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_lockDictClients);
                    }
                }
            }
            else
                logger.Warn("RegisterClient() duplicate client:{0}", clientId);
        }

        public void RegisterClient(string clientId, string serverUrl, MessageReceivedHandler onMsgReceived)
        {
            RegisterClient(clientId, serverUrl, null, onMsgReceived, null);
        }

        public void UnRegisterClient(string clientId)
        {
            logger.Info("UnRegisterClient() id:{0}", clientId);

            if (_dictClients.ContainsKey(clientId))
            {
                if (Monitor.TryEnter(_lockDictClients, DEFAULT_TIMEOUT_MS))
                {
                    try
                    {
                        if (_dictClients.ContainsKey(clientId))
                        {
                            TVUWebSocketClient2 client = _dictClients[clientId];

                            client.GetInitMessage -= client.GetInitMessage;
                            client.MessageReceived -= client.MessageReceived;

                            if (client.IsConnected)
                                client.Disconnect();

                            client.GetExitMessage -= client.GetExitMessage;
                            _dictClients.Remove(clientId);
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_lockDictClients);
                    }
                }
            }
            else
                logger.Warn("UnRegisterClient() client:{0} not exist.", clientId);
        }

        public bool SendMessage(string clientId, string msg)
        {
            bool ret = false;
            logger.Debug("SendMessage() client id:{0} msg:{1}", clientId, msg);

            if (!string.IsNullOrEmpty(msg) && _dictClients.ContainsKey(clientId))
            {
                if (Monitor.TryEnter(_lockDictClients, DEFAULT_TIMEOUT_MS))
                {
                    try
                    {
                        if (_dictClients.ContainsKey(clientId))
                            ret = _dictClients[clientId].Send(msg);
                    }
                    finally
                    {
                        Monitor.Exit(_lockDictClients);
                    }
                }
            }

            logger.Info("SendMessage() client id:{0} ret:{1}", clientId, ret);
            return ret;
        }

        public bool SendMessage(string clientId, byte[] data, int offset, int length)
        {
            bool ret = false;
            logger.Debug("SendMessage() client id:{0} data array length:{1} offset:{2} length:{3}", clientId, data == null ? 0 : data.Length, offset, length);

            if (data != null && data.Length > 0 && _dictClients.ContainsKey(clientId))
            {
                if (Monitor.TryEnter(_lockDictClients, DEFAULT_TIMEOUT_MS))
                {
                    try
                    {
                        if (_dictClients.ContainsKey(clientId))
                        {
                            logger.Debug("SendMessage() enter.");
                            ret = _dictClients[clientId].Send(data, offset, length);
                            logger.Debug("SendMessage() leave.");
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_lockDictClients);
                    }
                }
            }

            logger.Info("SendMessage() client id:{0} ret:{1}", clientId, ret);
            return ret;
        }

        public bool SendMessage(string clientId, byte[] data)
        {
            return _dictClients[clientId].Send(data, 0, data.Length);
        }

        #endregion
    }
}
