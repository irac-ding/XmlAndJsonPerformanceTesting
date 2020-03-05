/* =============================================
 * Copyright 2018 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: IWebSocketClientProxy.cs
 * Purpose:  Interface for websocket client proxy.
 * Author:   MikkoXU added on Mar.24th, 2018.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

namespace TVU.SharedLib.GenericUtility
{
    public delegate string GetInitMsgHandler();
    public delegate string GetExitMsgHandler();
    public delegate void MessageReceivedHandler(string id, string rawMessage);

    public interface IWebSocketClientProxy
    {
        void RegisterClient(string clientId, string serverUrl, GetInitMsgHandler getInitMsg, MessageReceivedHandler onMsgReceived, GetExitMsgHandler getExitMsg, bool isCheckStatus = false, int checkStatusInv = 2, int timeoutSeconds = 20);

        void UnRegisterClient(string clientId);

        bool SendMessage(string clientId, string msg);
    }
}
