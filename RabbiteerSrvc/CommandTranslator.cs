using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    class CommandTranslator
    {
        private AmqpHandler amqpHandler;

        public CommandTranslator(AmqpHandler amqpHandler)
        {
            this.amqpHandler = amqpHandler;
        }

        public void Translate(BindCommand command)
        {
            QueueDeclareOk ok = amqpHandler.model.QueueDeclare(command.QueueName, true, false, false, null);
            Console.WriteLine("{0}", ok);
            amqpHandler.model.QueueBind(command.QueueName, command.Exchange, command.RoutingKey, null);
        }


        public void Translate(PublishCommand command)
        {
            IBasicProperties props = amqpHandler.model.CreateBasicProperties();
            props.AppId = "Rabbiteer";
            props.Headers = command.Headers;
            props.ContentType = MimeType.GetMimeType(command.File);
            if (props.ContentType.StartsWith("text/"))
            {
                // XXX is this good? are ttnitf iso8859-1?
                props.ContentEncoding = "UTF-8";
            }
            byte[] body;
            try
            {
                string path = Path.GetFullPath(command.File);
                body = File.ReadAllBytes(path);
            }
            catch (FileNotFoundException fnfe)
            {
                Console.WriteLine("File not found {0}", command.File);
                return;
            }
            amqpHandler.model.BasicPublish(command.Exchange, command.RoutingKey, props, body);
        }
    }
}
