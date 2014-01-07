using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
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

        private Dictionary<string, BindRecord> bindings = new Dictionary<string, BindRecord>();

        public CommandTranslator(AmqpHandler amqpHandler)
        {
            this.amqpHandler = amqpHandler;
        }

        public void Translate(BindCommand command)
        {
            BindRecord record;
            bindings.TryGetValue(command.QueueName, out record);
            if (record != null)
            {
                if (record.command.GetHashCode() != command.GetHashCode())
                {
                    Console.WriteLine("Ignoring different bind settings for queue name: {0}", command.QueueName);
                }
                return;
            }

            // create new record which implicitly creates a consumer
            record = new BindRecord(command);

            // get a new model
            IModel model = amqpHandler.Model();

            try
            {
                model.QueueDeclare(command.QueueName, true, false, true, null);
            }
            catch (OperationInterruptedException e)
            {
                Console.WriteLine("Ignoring bind command, failed to declare queue: {0}", e.Message);
                return;
            }
            try
            {
                model.ExchangeDeclarePassive(command.Exchange);
                model.QueueBind(command.QueueName, command.Exchange, command.RoutingKey, null);
                model.BasicConsume(command.QueueName, true, record.consumer);
            }
            catch (OperationInterruptedException e)
            {
                if (e.ShutdownReason.ReplyCode == 404)
                {
                    Console.WriteLine("Ignoring bind command, no such exchange: {0}", command.Exchange);
                    return;
                }
                else
                {
                    Console.WriteLine(e.Message);
                    return;
                }
            }

            Console.WriteLine("Queue '{0}' bound to exchange '{1}' with routingKey '{2}'. Output to: {3}.", command.QueueName,
                command.Exchange, command.RoutingKey, command.OutDir);

            // remember we have this binding
            bindings.Add(command.QueueName, record);
        }

        private class BindRecord
        {
            public BindCommand command { get; private set; }
            public OutputConsumer consumer { get; private set; }

            public BindRecord(BindCommand command)
            {
                this.command = command;
                this.consumer = new OutputConsumer(command.OutDir);
            }
        }

        private class OutputConsumer : DefaultBasicConsumer
        {

            string outdir;

            public OutputConsumer(string outdir)
            {
                this.outdir = outdir;
            }

            public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
                string exchange, string routingKey, IBasicProperties properties, byte[] body)
            {
                try
                {
                    writeFile(body);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to write file: {0}", e.Message);
                }
            }

            private void writeFile(byte[] body)
            {
                Console.WriteLine("XXX Write file to {0}", outdir);
            }
        }


        public void Translate(PublishCommand command)
        {
            IModel model = amqpHandler.Model();
            IBasicProperties props = model.CreateBasicProperties();
            props.AppId = "Rabbiteer";
            props.Headers = command.Headers;
            props.ContentType = MimeType.GetMimeType(command.File);
            if (props.ContentType.StartsWith("text/"))
            {
                // XXX is this good? are ttnitf iso8859-1?
                // we need to check xml and html files for headers.
                props.ContentEncoding = "UTF-8";
            }
            byte[] body;
            try
            {
                string path = Path.GetFullPath(command.File);
                if (!File.Exists(path))
                {
                    Console.WriteLine("File not found {0}", command.File);
                    return;
                }
                body = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read file '{0}': {1}", command.File, e.Message);
                return;
            }
            model.BasicPublish(command.Exchange, command.RoutingKey, props, body);
        }
    }
}
