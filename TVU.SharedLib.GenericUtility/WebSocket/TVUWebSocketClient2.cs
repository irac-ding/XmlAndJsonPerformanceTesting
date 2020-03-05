/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: TVUWebSocketClient2.cs
 * Purpose:  Client connected to certain web service via WebSocket for duplex communication,
 *           to reduce socket expense and TIME_WAIT count.
 * Note:     Use https://github.com/sta/websocket-sharp
 *           instead of https://github.com/kerryjiang/WebSocket4Net to reduce dependencies.
 * Author:   MikkoXU added on Mar.19th, 2018, but the main structure is inherited from TVUWebSocketClient.cs.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Threading;
using Newtonsoft.Json;
using NLog;
using WebSocketSharp;
using System.Threading.Tasks;
using Logger = NLog.Logger;

namespace TVU.SharedLib.GenericUtility
{
    internal class TVUWebSocketClient2
    {
        #region Log

        private Logger logger { get; } = LogManager.GetCurrentClassLogger();
        private Logger loggerStatus { get; } = LogManager.GetLogger("TVU.WebSocketClient2.CommunicationStatus");
        private Logger loggerRecMsg { get; } = LogManager.GetLogger("TVU.WebSocketClient2.RecMessage");
        private Logger loggerSendMsg { get; } = LogManager.GetLogger("TVU.WebSocketClient2.SendMessage");

        #endregion

        #region Delegates

        [JsonIgnore]
        public GetInitMsgHandler GetInitMessage { get; set; }

        [JsonIgnore]
        public GetExitMsgHandler GetExitMessage { get; set; }

        [JsonIgnore]
        public MessageReceivedHandler MessageReceived { get; set; }

        #endregion

        #region Fields

        private WebSocket _webSocket;

        private const int DEFAULT_TIMEOUT_MS = 2000;

        private bool IsRegisterEvent { get; set; }

        private static readonly object _lockWebSocketClient = new object();

        #endregion

        #region Fields And Properties

        /// <summary>
        /// Use ping-pong instead of application-layer heartbeat.
        /// See RFC6455.
        /// Only for compatibility with old servers here.
        /// </summary>
        [Obsolete]
        private const string DEFAULT_HEARTBEAT_MSG = "heartbeat";

        private const int DEFAULT_DELAY_MS = 1000;

        /// <summary>
        /// Default ping-pong interval, in seconds.
        /// </summary>
        private const int DEFAULT_PING_PONG_INTERVAL = 10;

        public string ID { get; private set; }

        public string ServerUrl { get; private set; }

        public int PingPongInterval { get; private set; }

        public string StrHeartBeatMsg { get; private set; }

        public int ReConnectDelayOnError { get; private set; }

        public int ReConnectDelayOnClosed { get; private set; }

        public bool IsConnected
        {
            get
            {
                if (_webSocket == null)
                    return false;
                return _webSocket.ReadyState == WebSocketState.Open;
            }
        }

        internal DateTime HeartBeatUpdateTime { get; set; } = DateTime.UtcNow;

        #endregion

        #region Constructor

        internal TVUWebSocketClient2(string id, string serverUrl, int pingPongInterval, string strHeartBeatMsg, int reConnectDelayOnError, int reConnectDelayOnClosed)
        {
            ID = id;
            if (serverUrl.EndsWith("/"))
                serverUrl = serverUrl.Remove(serverUrl.Length - 1);
            ServerUrl = serverUrl;

            PingPongInterval = pingPongInterval;
            StrHeartBeatMsg = strHeartBeatMsg;

            ReConnectDelayOnError = reConnectDelayOnError;
            ReConnectDelayOnClosed = reConnectDelayOnClosed;
        }

        internal TVUWebSocketClient2(string id, string serverUrl)
            : this(id, serverUrl, DEFAULT_PING_PONG_INTERVAL, DEFAULT_HEARTBEAT_MSG, DEFAULT_DELAY_MS, DEFAULT_DELAY_MS)
        {
        }

        ~TVUWebSocketClient2()
        {
            Disconnect();
        }

        #endregion

        #region Methods

        public void Connect()
        {
            Disconnect();
            //Thread.Sleep(1000);
            Task.Factory.StartNew(() =>
            {
                if (Monitor.TryEnter(_lockWebSocketClient, DEFAULT_TIMEOUT_MS))
                {
                    try
                    {
                        if (_webSocket == null)
                        {
                            Uri serverUri = new Uri(ServerUrl, UriKind.Absolute);
                            _webSocket = new WebSocket(serverUri.ToString());
                        }
                        logger.Debug("Connect() id:{0} url: {1}.", ID, ServerUrl);

                        if (_webSocket != null)
                        {
                            if (!IsRegisterEvent)
                            {
                                IsRegisterEvent = true;
                                _webSocket.OnOpen += webSocket_Opened;
                                _webSocket.OnError += webSocket_Error;
                                _webSocket.OnClose += webSocket_Closed;
                                _webSocket.OnMessage += webSocket_MessageReceived;
                            }

                            loggerStatus.Info($"Connect() id: call _webSocket.Open() _webSocket.State: {_webSocket.ReadyState}, time: {DateTime.Now:O}.");
                            // TODO: Confirm state check.
                            if (_webSocket.ReadyState != WebSocketState.Open)
                                _webSocket?.Connect();
                            loggerStatus.Info($"Connect() id:{ID} call _webSocket.Open() done _webSocket.State: {_webSocket.ReadyState}, time: {DateTime.Now:O}.");
                            HeartBeatUpdateTime = DateTime.UtcNow;
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerStatus.Error("Connect() id:{0} call _webSocket.Open() error: {1}.", ID, ex.Message);
                    }
                    finally
                    {
                        Monitor.Exit(_lockWebSocketClient);
                    }
                }
            });
        }

        public void Disconnect()
        {
            Task.Factory.StartNew(() =>
            {
                if (Monitor.TryEnter(_lockWebSocketClient, DEFAULT_TIMEOUT_MS))
                {
                    try
                    {
                        if (_webSocket == null) return;
                        if (GetExitMessage != null)
                        {
                            string exitMsg = GetExitMessage();
                            if (!string.IsNullOrWhiteSpace(exitMsg))
                            {
                                Send(exitMsg);
                                loggerSendMsg.Info($"Disconnect() id:{ID} exit msg:{exitMsg}, time: {DateTime.Now:O}");
                            }
                        }

                        if (IsRegisterEvent)
                        {
                            _webSocket.OnOpen -= webSocket_Opened;
                            _webSocket.OnError -= webSocket_Error;
                            _webSocket.OnClose -= webSocket_Closed;
                            _webSocket.OnMessage -= webSocket_MessageReceived;
                            IsRegisterEvent = false;
                        }
                        DateTime now = DateTime.Now;
                        loggerStatus.Info($"Disconnect() id:{ID}, time: {now:O} call _webSocket.Close() _webSocket.State: {_webSocket.ReadyState}.");
                        // TODO: Confirm state check.
                        if (_webSocket.ReadyState != WebSocketState.Closed)
                            _webSocket?.Close();
                        now = DateTime.Now;
                        loggerStatus.Info($"Disconnect() id:{ID}, time: {now:O} call _webSocket.Close() done _webSocket.State: {_webSocket.ReadyState}.");
                        _webSocket = null;
                        HeartBeatUpdateTime = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        loggerStatus.Error("Disconnect() id:{0} call _webSocket.Close() error: {1}.", ID, ex.Message);
                    }
                    finally
                    {
                        Monitor.Exit(_lockWebSocketClient);
                    }
                }
            });
        }

        public bool Send(string msg)
        {
            DateTime now = DateTime.Now;
            loggerSendMsg.Debug($"Send() id:{ID}, time: {now:O} msg:{msg} websocket connected:{IsConnected}");
            bool ret = false;

            try
            {
                if (IsConnected && !string.IsNullOrEmpty(msg))
                {
                    _webSocket.Send(msg);
                    ret = true;
                }

                loggerSendMsg.Debug($"Send() id:{ID}, time: {DateTime.Now:O} ret:{ret}.");
                return ret;
            }
            catch (Exception ex)
            {
                loggerSendMsg.Error("Send() id:{0} call _webSocket.Send() error: {1}.", ID, ex.Message);
                return false;
            }
        }

        public bool Send(byte[] data, int offset, int length)
        {
            DateTime now = DateTime.Now;
            loggerSendMsg.Debug($"Send() id:{ID}, time: {now:O} data array length:{data?.Length ?? 0} offset:{offset} length:{length} websocket connected:{IsConnected}");

            bool ret = false;

            try
            {
                if (IsConnected && data != null && data.Length > 0)
                {
                    _webSocket.Send(data);
                    ret = true;
                }

                loggerSendMsg.Debug("Send() id:{0} ret:{1}.", ID, ret);
                return ret;
            }
            catch (Exception ex)
            {
                loggerSendMsg.Error("Send() id:{0} call _webSocket.Send() error:{1}.", ID, ex.Message);
                return false;
            }
        }

        public bool Send(byte[] data)
        {
            return Send(data, 0, data.Length);
        }

        #endregion

        #region WebSocket event handlers

        private void webSocket_Opened(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                loggerStatus.Info($"webSocket_Opened() id:{ID} time: {now:O} url:{ServerUrl} opened.");

                if (GetInitMessage != null)
                {
                    string initMsg = GetInitMessage();
                    loggerSendMsg.Info($"webSocket_Opened() id:{ID} time: {now:O} get init msg:{initMsg}.");
                    if (!string.IsNullOrEmpty(initMsg))
                        Send(initMsg);
                }
            }
            catch (Exception ex)
            {
                loggerStatus.Error("webSocket_Opened() id:{0} error:{1}.", ID, ex.Message);
            }
        }

        private void webSocket_Error(object sender, ErrorEventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                loggerStatus.Info($"webSocket_Error() id:{ID} time: {now:O} url:{ServerUrl} error {e.Exception.Message}.");
            }
            catch (Exception ex)
            {
                loggerStatus.Error("webSocket_Error() id:{0} error:{1}.", ID, ex.Message);
            }
        }

        private void webSocket_Closed(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                loggerStatus.Info($"webSocket_Closed() id:{ID}, time: {now:O} url:{ServerUrl} closed.");

                Thread.Sleep(ReConnectDelayOnClosed);
                Connect();
            }
            catch (Exception ex)
            {
                loggerStatus.Error("webSocket_Closed() id:{0} error:{1}.", ID, ex.Message);
            }
        }

        private void webSocket_MessageReceived(object sender, MessageEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                DateTime now = DateTime.Now;
                loggerRecMsg.Trace($"webSocket_MessageReceived(), time: {now:O} id:{ID} url:{ServerUrl} received message:{e.Data}.");

                if (e.Data == StrHeartBeatMsg)
                {
                    HeartBeatUpdateTime = DateTime.UtcNow;
                    return;
                }

                if (MessageReceived != null)
                {
                    loggerRecMsg.Debug($"webSocket_MessageReceived() id:{ID} msg:{e.Data}, time: {now:O}");
                    MessageReceived(ID, e.Data);
                }
            }
            catch (Exception ex)
            {
                loggerStatus.Error("webSocket_MessageReceived() id:{0} error:{1}.", ID, ex.Message);
            }
        }

        #endregion

        #region Json

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        #endregion
    }
}
