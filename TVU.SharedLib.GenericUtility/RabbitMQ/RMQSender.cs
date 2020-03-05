/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: RMQSender.cs
 * Author:   MikkoXU added on sometime Aug 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Text;
using NLog;

namespace TVU.SharedLib.GenericUtility.RabbitMQ
{
    /// <summary>
    /// A simple RabbitMQ sender using named queue.
    /// </summary>
    /// <see cref="https://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html" />
    public class RMQSender : RMQClient
    {
        #region Log

        protected override Logger logger { get; } = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields and properties

        private string _queue;

        #endregion

        #region .ctors

        public RMQSender(RMQConnection connection, string queue)
            : base(connection)
        {
            _queue = queue;
            Channel.QueueDeclare(_queue,
                false,
                false,
                false,
                null);

            logger.Info($"RMQSender with queue {queue} created.");
        }

        #endregion

        #region Methods

        public void Send(string message)
        {
            try
            {
                byte[] body = Encoding.UTF8.GetBytes(message);
                Channel.BasicPublish("",
                    _queue,
                    null,
                    body);
            }
            catch (Exception ex)
            {
                logger.Error("Send() error, {0}.", ex.Message);
            }
        }

        #endregion
    }
}
