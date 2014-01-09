using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Channels.Ipc;
using System.Collections;
using System.Runtime.Remoting;
using System.Threading;
using System.Security.Principal;

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

            Log.Info("Opening service channel");

            IDictionary prop = new Hashtable();
            prop["name"] = "RabbiteerSrvc";
            prop["portName"] = "localhost:" + SERVICE_PORT;
            prop["tokenImpersonationLevel"] = TokenImpersonationLevel.Impersonation;
            prop["includeVersions"] = false;
            prop["strictBinding"] = false;
            prop["secure"] = true;
            prop["authorizedGroup"] = "Everyone";

            try
            {
                serverChannel = new IpcChannel(prop, null, null);
            }
            catch (RemotingException e)
            {
                Log.Error("Failed to open channel: {0}", e.Message);
                // fatal
                throw e;
            }

            Log.Info("Registering channel");

            // Register the server channel.
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(
                serverChannel, false);

            Log.Info("Creating command queue singleton");

            // singleton
            queue = new RemoteCommandQueue();

            Log.Info("Exporting singleton");

            // export singleton
            RemotingServices.Marshal(queue, "RabbiteerRemoteCommandQueue");  

            // Parse the channel's URI. 
            string[] urls = serverChannel.GetUrlsForUri("RabbiteerRemoteCommandQueue");
            if (urls.Length > 0)
            {
                Log.Info("The object URL is {0}.", urls[0]);
            }

            Log.Info("Creating AMQP handler");

            amqpHandler = new AmqpHandler();
            amqpHandler.Open += delegate(bool isOpen)
            {
                queue.acceptCommands = isOpen;
            };
            queue.acceptCommands = amqpHandler.isOpen();

            Log.Info("Creating command handler");

            CommandHandler handler = new CommandHandler(amqpHandler);

            new Thread(delegate()
            {
                dynamic comm;
                while ((comm = queue.GetCommand()) != null)
                {
                    while (true)
                    {
                        if (handler.Translate(comm)) break;
                        Thread.Sleep(3000);
                    }
                }
            }).Start();

            Log.Info("Server is up");

        }

        public void DoStop()
        {
            if (serverChannel != null) System.Runtime.Remoting.Channels.ChannelServices.UnregisterChannel(serverChannel);
            serverChannel = null;
            if (amqpHandler != null) amqpHandler.shutdown();
            amqpHandler = null;
            // null shuts down thread
            queue.AddCommand(null);
        }

    }
}
