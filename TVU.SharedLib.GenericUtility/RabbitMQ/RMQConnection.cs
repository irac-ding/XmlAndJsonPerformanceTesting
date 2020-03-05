/* =============================================
 * Copyright 2019 TVU Networks Co.,Ltd. All rights reserved.
 * For internal members in TVU Networks only.
 * FileName: RMQConnection.cs
 * Author:   MikkoXU added on sometime Aug 2019.
 * Since:    Microsoft Visual Studio 2015
 * =============================================*/

using System;
using NLog;
using RabbitMQ.Client;

namespace TVU.SharedLib.GenericUtility.RabbitMQ
{
    /// <summary>
    /// Holds the RabbitMQ connection and shares it inside the process.
    /// I don't expect multiple RabbitMQ brokers on one machine for local use. But there's a chance that other central services may use RMQ as well.
    /// </summary>
    public class RMQConnection
    {
        #region Log

        private Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Fields and properties

        private ConnectionFactory _factory = null;

        public IConnection Connection { get; private set; }

        #endregion

        #region Methods

        public void Init(string host = "localhost", int port = 5672)
        {
            _factory = new ConnectionFactory() { HostName = host, Port = port };

            try
            {
                Connection = _factory.CreateConnection();
                logger.Info($"Init() to {host}, port {port}.");
            }
            catch (Exception ex)
            {
                logger.Info("Init() to {0}, port {1} error, {2}.", host, port, ex.Message);
            }
        }

        public void ShutdownSystem()
        {
            logger.Info($"ShutdownSystem() to {_factory?.HostName}:{_factory?.Port}.");

            Connection?.Dispose();
            Connection = null;
            _factory = null;

            logger.Info("ShutdownSystem() done.");
        }

        #endregion
    }
}
