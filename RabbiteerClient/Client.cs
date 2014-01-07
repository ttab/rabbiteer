using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    class Client
    {

        private RemoteCommandQueue queue;

        [SecurityPermission(SecurityAction.Demand)]
        public Client()
        {
            // Create the channel.
            IpcChannel channel = new IpcChannel();

            // Register the channel.
            System.Runtime.Remoting.Channels.ChannelServices.
                RegisterChannel(channel, true);

            // Register as client for remote object.
            System.Runtime.Remoting.WellKnownClientTypeEntry remoteType =
                new System.Runtime.Remoting.WellKnownClientTypeEntry(
                    typeof(RemoteCommandQueue),
                    "ipc://localhost:" + Server.SERVICE_PORT + "/RabbiteerRemoteCommandQueue");
            System.Runtime.Remoting.RemotingConfiguration.
                RegisterWellKnownClientType(remoteType);

            // Create an instance of the remote object.
            queue = new RemoteCommandQueue(); 

        }

        public bool AddCommand(Command command)
        {
            return queue.AddCommand(command);
        }

    }
}
