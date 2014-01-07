using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    class AmqpHandler
    {

        private ConnectionFactory factory;
        private IConnection connection;
        public event OpenHandler Open;
        public delegate void OpenHandler(bool isOpen);

        public AmqpHandler()
        {
            factory = new ConnectionFactory();
            string uri = ConfigurationManager.AppSettings["amqpUri"].ToString();
            Console.WriteLine("Using AMQP URI: {0}", uri);
            factory.Uri = uri;
            connect();
        }

        public bool isOpen()
        {
            return connection != null;
        }

        public IModel Model()
        {
            return connection != null ? connection.CreateModel() : null;
        }

        private void connect()
        {
            if (connection != null)
            {
                disconnect();
            }
            connection = factory.CreateConnection();
            if (Open != null) Open(true);
            Console.WriteLine("AMQP Connected");
        }

        private void disconnect()
        {
            if (connection == null) return;
            if (Open != null) Open(false);
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
            Console.WriteLine("AMQP Closed");
        }

        public void shutdown()
        {
            disconnect();
        }

    }
}
