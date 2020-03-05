/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: RMQClient.cs
 * Author:   MikkoXU added on sometime Aug 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using NLog;
using RabbitMQ.Client;

namespace TVU.SharedLib.GenericUtility.RabbitMQ
{
    /// <summary>
    /// Base class of all RabbitMQ clients.
    /// </summary>
    public abstract class RMQClient
    {
        #region Log

        protected virtual Logger logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields and properties

        protected IModel Channel { get; set; }

        #endregion

        #region .ctors

        public RMQClient(RMQConnection connection)
        {
            Channel = connection.Connection.CreateModel();
        }

        #endregion

        public virtual void Init()
        {
        }

        public virtual void ShutdownSystem()
        {
            Channel?.Dispose();
            Channel = null;

            // TODO: Maybe I should add a nickname for each client?
        }
    }
}
