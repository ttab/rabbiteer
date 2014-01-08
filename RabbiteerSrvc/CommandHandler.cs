using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    class CommandHandler
    {
        private AmqpHandler amqpHandler;

        private Dictionary<string, BindRecord> bindings = new Dictionary<string, BindRecord>();

        public CommandHandler(AmqpHandler amqpHandler)
        {
            this.amqpHandler = amqpHandler;
            amqpHandler.Open += delegate(bool isOpen)
            {
                if (isOpen) HandleRebind();
            };
        }

        private void HandleRebind() {
            foreach (BindRecord rec in bindings.Values) {
                IModel model = amqpHandler.Model();
                if (model != null)
                {
                    rec.model = model;
                    if (!rec.connect())
                    {
                        Console.WriteLine("Rebind failed");
                        return;
                    }
                    Console.WriteLine("Rebound: {0}", rec.command.QueueName);
                }
            }
        }

        public bool Translate(BindCommand command)
        {
            BindRecord record;
            bindings.TryGetValue(command.QueueName, out record);
            if (record != null)
            {
                if (record.command.GetHashCode() != command.GetHashCode())
                {
                    Console.WriteLine("Ignoring different bind settings for queue name: {0}", command.QueueName);
                }
                return true;
            }

            // create new record which implicitly creates a consumer
            record = new BindRecord(command);

            // get a new model
            IModel model = amqpHandler.Model();
            if (model == null) return false;

            // hook up model
            record.model = model;

            // and construct
            if (!record.connect())
            {
                return false;
            }

            Console.WriteLine("Queue '{0}' bound to exchange '{1}' with routingKey '{2}'. Output to: {3}.", command.QueueName,
                command.Exchange, command.RoutingKey, command.OutDir);

            // remember we have this binding
            bindings.Add(command.QueueName, record);

            return true;
        }

        private class BindRecord
        {
            public BindCommand command { get; private set; }
            public OutputConsumer consumer { get; private set; }
            private IModel _model;
            public IModel model
            {
                get
                {
                    return _model;
                }
                set
                {
                    _model = value;
                    consumer.model = value;
                }
            }

            public BindRecord(BindCommand command)
            {
                this.command = command;
                this.consumer = new OutputConsumer(command.OutDir);
            }


            internal bool connect()
            {
                try
                {
                    model.QueueDeclare(command.QueueName, true, false, true, null);
                }
                catch (OperationInterruptedException e)
                {
                    Console.WriteLine("Ignoring bind command, failed to declare queue: {0}", e.Message);
                    return false;
                }
                try
                {
                    model.ExchangeDeclarePassive(command.Exchange);
                    model.QueueBind(command.QueueName, command.Exchange, command.RoutingKey, null);
                    // start consuming
                    model.BasicConsume(command.QueueName, false, consumer);
                }
                catch (OperationInterruptedException e)
                {
                    if (e.ShutdownReason.ReplyCode == 404)
                    {
                        Console.WriteLine("Ignoring bind command, no such exchange: {0}", command.Exchange);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
                // all good
                return true;
            }
        }

        private class OutputConsumer : DefaultBasicConsumer
        {

            string outdir;
            public IModel model {get; set;}

            public OutputConsumer(string outdir)
            {
                this.outdir = outdir;
            }

            public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
                string exchange, string routingKey, IBasicProperties properties, byte[] body)
            {
                object oFileName;
                string fileName = null;
                properties.Headers.TryGetValue("fileName", out oFileName);
                if (oFileName != null )
                {
                    try
                    {
                        fileName = System.Text.Encoding.UTF8.GetString((byte[])oFileName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to decode 'fileName' header as UTF-8: {0}", e.Message);
                    }
                }
                string contentType = properties.ContentType;
                if (contentType == null)
                {
                    if (fileName != null) {
                        contentType = MimeType.GetMimeType(fileName);
                    } else {
                        // last fallback
                        contentType = "application/octet-stream";
                    }
                }
                bool ok = DoHandle(body, fileName, contentType);
                if (ok)
                {
                    model.BasicAck(deliveryTag, false);
                }
                else
                {
                    model.BasicReject(deliveryTag, true);
                }
            }

            private bool DoHandle(byte[] body, string fileName, string contentType)
            {
                if (!Directory.Exists(outdir))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(outdir);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to create outdir '{0}': {1}", outdir, e.Message);
                        return false;
                    }
                }
                int i = 0;
                string path = null;
                string baseName;
                if (fileName == null)
                {
                    fileName = DateTime.Now.ToString("yyyyMMdd-HHmmss") + MimeType.GetExtension(contentType);
                }
                int idx = fileName.LastIndexOf('.');
                if (idx >= 0) {
                    baseName = fileName.Substring(0, idx) + "-{0:00}" + fileName.Substring(idx); 
                } else {
                    baseName = fileName + "-{0:00}";
                }
                while (true)
                { 
                    while (true)
                    {
                        if (fileName == null || i > 0)
                            fileName = String.Format(baseName, i);
                        path = Path.Combine(outdir, fileName);
                        i++;
                        if (i > 99)
                        {
                            Console.WriteLine("Failed to find unique filename: {0}", path);
                            return false;
                        }
                        if (File.Exists(path)) continue;
                        // XXX atomic creation here
                        break;
                    }
                    try
                    {
                        writeFile(body, path);
                        break;
                    }
                    catch (FileAlreadyExistException)
                    {
                        // try new file name
                        continue;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to write file: {0}", e.Message);
                        return false;
                    }
                }
                return true;
            }

            // XXX can be removed if we do atomic creation above
            [MethodImpl(MethodImplOptions.Synchronized)]
            private void writeFile(byte[] body, string path)
            {
                if (File.Exists(path))
                    throw new FileAlreadyExistException();
                File.WriteAllBytes(path, body);
            }
        }

        internal class FileAlreadyExistException : IOException
        {

        }

        public bool Translate(PublishCommand command)
        {
            IModel model = amqpHandler.Model();
            if (model == null) return false;
            IBasicProperties props = model.CreateBasicProperties();
            props.AppId = "Rabbiteer";
            props.Headers = command.Headers;
            props.Headers.Add("fileName", Path.GetFileName(command.File));
            props.ContentType = MimeType.GetMimeType(command.File);
            if (props.ContentType.StartsWith("text/"))
            {
                // XXX is this good? are ttnitf iso8859-1?
                // we need to check xml and html files for headers.
                props.ContentEncoding = "UTF-8";
            }
            byte[] body;
            string path;
            try
            {
                path = Path.GetFullPath(command.File);
                if (!File.Exists(path))
                {
                    Console.WriteLine("File not found {0}", command.File);
                    return false;
                }
                body = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to read file '{0}': {1}", command.File, e.Message);
                return false;
            }

            if (!Directory.Exists(command.ReadyDir))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(command.ReadyDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create dir '{0}': {1}", command.ReadyDir, e.Message);
                    return false;
                }
            }

            try {
                // send off
                model.BasicPublish(command.Exchange, command.RoutingKey, props, body);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to publish: {0}", e.Message);
                return false;
            }

            // move file
            string curDir = Directory.GetParent(path).ToString();
            if (curDir != command.ReadyDir)
            {
                try
                {
                    string name = Path.GetFileName(path);
                    string target = Path.Combine(command.ReadyDir, name);
                    Directory.Move(path, target);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to move file '{0}' to dir '{1}': {2}", command.File, command.ReadyDir, e.Message);
                    return false;
                }
            }

            return true;
        }
    }
}
