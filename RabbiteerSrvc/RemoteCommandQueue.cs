using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{

    public class RemoteCommandQueue : MarshalByRefObject
    {
        public bool acceptCommands {get; set;}
        private BlockingCollection<Command> queue = new BlockingCollection<Command>();

        public bool AddCommand(Command command)
        {
            if (!acceptCommands)
            {
                Console.WriteLine("Service did not accept command.");
                return false;
            }
            queue.Add(command);
            return true;
        }

        public Command GetCommand()
        {
            return queue.Take();
        }

        // to not get garbage collected
        public override object InitializeLifetimeService() { return (null); }

    }
}
