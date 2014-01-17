using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rabbiteer
{
    class AmqpHandler
    {

        private ConnectionFactory factory;
        private IConnection connection;
        private IModel model;
        private bool reconnecting = false;
        public event OpenHandler Open;
        public delegate void OpenHandler(bool isOpen);

        public AmqpHandler()
        {
            try
            {
                factory = new ConnectionFactory();
                string host = Config.INSTANCE.GetValue("host", "amqp");
                string vhost = Config.INSTANCE.GetValue("vhost", "amqp");
                string login = Config.INSTANCE.GetValue("login", "amqp");
                string password = Config.INSTANCE.GetValue("password", "amqp");
                string uri = String.Format("amqp://{0}:{1}@{2}/{3}", login, password, host, vhost);
                Log.Info("Using AMQP URI: {0}", uri);
                factory.Uri = uri;
            }
            catch (Exception e)
            {
                Log.Error("Failed to configure AMQP connection: {0}", e.Message);
                // this is fatal
                throw e;
            }
            connect();
        }

        public bool isOpen()
        {
            return connection != null;
        }

        public IModel Model()
        {
            try {
                if (connection == null) {
                    return null;
                }
                if (model == null || !model.IsOpen)
                {
                    doCloseModel();
                    model = connection.CreateModel();
                }
                return model != null ? model : null;
            }
            catch (OperationInterruptedException e)
            {
                Log.Info("AMQP failed: {0}", e.Message);
                disconnect();
                startReconnect();
                return null;
            }
        }

        private void doCloseModel() {
            if (model == null) return;
            try
            {
                model.Abort();
            }
            catch (Exception)
            {
                // ignore
            }
            model = null;
        }

        private bool connect()
        {
            if (connection != null)
            {
                disconnect();
            }
            try
            {
                connection = factory.CreateConnection();
            }
            catch (OperationInterruptedException e)
            {
                Log.Info("Connect failed: {0}", e.Message);
                startReconnect();
                return false;
            }
            catch (BrokerUnreachableException e)
            {
                Log.Info("Connect failed: {0}", e.Message);
                startReconnect();
                return false;
            }
            reconnecting = false;
            if (Open != null) Open(true);
            Log.Info("AMQP Connected");
            return true;
        }

        private void disconnect()
        {
            if (connection == null) return;
            if (Open != null) Open(false);
            doCloseModel();
            try
            {
                connection.Abort(5);
            }
            catch (AlreadyClosedException) { 
                // ye ye
            }
            catch (IOException) {
                // unexpected close
            }
            connection = null;
            Log.Info("AMQP Closed");
        }

        public void shutdown()
        {
            reconnecting = false;
            disconnect();
        }

        private void startReconnect()
        {
            if (reconnecting) return;
            reconnecting = true;
            new Thread(delegate() {
                while (reconnecting)
                {
                    if (connect()) return;
                    Thread.Sleep(3000);
                }
            }).Start();
        }

    }
}
