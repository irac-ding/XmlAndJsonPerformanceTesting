/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: RMQSubscriber.cs
 * Author:   MikkoXU added on sometime Aug 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client.Events;

namespace TVU.SharedLib.GenericUtility.RabbitMQ
{
    /// <summary>
    /// A RabbitMQ subscriber receives message from some publisher.
    /// </summary>
    /// <see cref="https://www.rabbitmq.com/tutorials/tutorial-three-dotnet.html" />
    public class RMQSubscriber : RMQClient
    {
        #region Fields and properties

        private string _exchange;

        private AutoResetEvent _exitEvent = new AutoResetEvent(false);

        #endregion

        #region .ctors

        public RMQSubscriber(RMQConnection connection, string exchange)
            : base(connection)
        {
            _exchange = exchange;
            Channel.ExchangeDeclare(_exchange,
                "fanout",
                true);

            logger.Info($"RMQSubscriber with exchange {exchange} created.");
        }

        #endregion

        #region Methods

        public void StartReceiving(Action<string> onMessageReceived)
        {
            try
            {
                string queueName = Channel.QueueDeclare().QueueName;
                Channel.QueueBind(queueName,
                    _exchange,
                    "");
                logger.Info($"RMQSubscriber with exchange {_exchange} queue bound.");

                EventingBasicConsumer consumer = new EventingBasicConsumer(Channel);
                consumer.Received += (model, ea) =>
                {
                    try
                    {
                        byte[] body = ea.Body;
                        string message = Encoding.UTF8.GetString(body);
                        onMessageReceived?.Invoke(message);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("RMQSubscriber with exchange {0} error, {1}.", _exchange, ex.Message);
                    }
                };

                Channel.BasicConsume(queueName,
                    false,
                    consumer);
                logger.Info($"RMQSubscriber with exchange {_exchange} start receiving.");

                _exitEvent.WaitOne();
                logger.Info($"RMQSubscriber with exchange {_exchange} stopped.");
            }
            catch (Exception ex)
            {
                logger.Error("StartReceiving() with exchange {0} error, {1}.", _exchange, ex.Message);
            }
        }

        public void Stop()
        {
            logger.Info($"RMQSubscriber with exchange {_exchange} stopping.");
            _exitEvent.Set();
        }

        #endregion
    }
}
