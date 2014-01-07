using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    [Serializable]
    public abstract class Command
    {
        public abstract bool Validate();
    }


    [Serializable]
    public class BindCommand : Command
    {
        public string QueueName { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public string OutDir { get; set; }

        public override bool Validate()
        {
            bool ok = true;
            if (QueueName == null)
            {
                Console.WriteLine("Missing QueueName");
                ok = false;
            }
            if (Exchange == null)
            {
                Console.WriteLine("Missing Exchange");
                ok = false;
            }
            if (RoutingKey == null)
            {
                Console.WriteLine("Missing RoutingKey");
                ok = false;
            }
            if (OutDir == null)
            {
                Console.WriteLine("Missing OutDir");
                ok = false;
            }
            return ok;
        }
    }

    [Serializable]
    public class PublishCommand : Command
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public IDictionary<string, object> Headers = new Dictionary<string, object>();
        public string File { get; set; }

        public override bool Validate()
        {
            bool ok = true;
            if (Exchange == null)
            {
                Console.WriteLine("Missing Exchange");
                ok = false;
            }
            if (RoutingKey == null)
            {
                Console.WriteLine("Missing RoutingKey");
                ok = false;
            }
            if (File == null)
            {
                Console.WriteLine("Missing File");
                ok = false;
            }
            return ok;
        }
    }

}
