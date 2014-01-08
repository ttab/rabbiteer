using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    class Program
    {
        static void Main(string[] args)
        {

            bool isBind = false;
            bool isPublish = false;

            var p = new OptionSet()
            {
                {"b|bind", v => isBind = true},
                {"p|publish", v => isPublish = true}
            };
            p.Parse(args);

            if (isBind)
            {
                DoBind(args);
            }
            else if (isPublish)
            {
                DoPublish(args);
            }
            else
            {
                Console.WriteLine("Usage: Rabbiteer.exe -b|-bind [options]");
                Console.WriteLine("                     -p|-publish [options]");
                Environment.Exit(-1);
            }

        }


        private static void DoBind(string[] args)
        {
            BindCommand c = new BindCommand();
            var p = new OptionSet()
            {
                {"q|queuename=", v => c.QueueName = v},
                {"e|exchange=", v => c.Exchange = v},
                {"r|routingkey=", v => c.RoutingKey = v},
                {"o|outdir=", v => c.OutDir = v},
            };
            p.Parse(args);
            if (!c.Validate())
            {
                Environment.Exit(-1);
            }
            DoAddCommand(c);
        }

        private static void DoPublish(string[] args)
        {
            PublishCommand c = new PublishCommand();
            char[] del = new char[]{'='};
            var p = new OptionSet()
            {
                {"e|exchange=", v => c.Exchange = v},
                {"r|routingkey=", v => c.RoutingKey = v},
                {"h|header=", v => c.Headers.Add(v.Split(del, 2)[0], v.Split(del, 2)[1])},
                {"f|file=", v => c.File = v},
                {"d|dir=", v => c.ReadyDir = v}
            };
            p.Parse(args);
            if (!c.Validate())
            {
                Environment.Exit(-1);
            }
            DoAddCommand(c);
        }

        private static void DoAddCommand(Command c)
        {
            bool r = (new Client()).AddCommand(c);
            Environment.Exit(r ? 0 : -1);
        }

    }
}
