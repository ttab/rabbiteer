using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Ipc;
using System.Collections;
using System.Runtime.Remoting;

namespace Rabbiteer
{
    public class Server
    {

        public const int SERVICE_PORT = 46617;

        private IpcChannel serverChannel;
        public RemoteCommandQueue queue { get; set; }
        private AmqpHandler amqpHandler;

        [SecurityPermission(SecurityAction.Demand)]
        public void DoStart()
        {

            serverChannel = new IpcChannel("localhost:" + SERVICE_PORT); 

            // Register the server channel.
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(
                serverChannel, true);

            // singleton
            queue = new RemoteCommandQueue();

            // export singleton
            RemotingServices.Marshal(queue, "RabbiteerRemoteCommandQueue");  

            // Parse the channel's URI. 
            string[] urls = serverChannel.GetUrlsForUri("RabbiteerRemoteCommandQueue");
            if (urls.Length > 0)
            {
                Console.WriteLine("The object URL is {0}.", urls[0]);
            }

            amqpHandler = new AmqpHandler();
            amqpHandler.Open += delegate(bool isOpen)
            {
                queue.acceptCommands = isOpen;
            };

            CommandTranslator commandTranslator = new CommandTranslator(amqpHandler);

        }

        public void DoStop()
        {
            if (serverChannel != null) System.Runtime.Remoting.Channels.ChannelServices.UnregisterChannel(serverChannel);
            serverChannel = null;
            if (amqpHandler != null) amqpHandler.shutdown();
            amqpHandler = null;
        }


    }
}
