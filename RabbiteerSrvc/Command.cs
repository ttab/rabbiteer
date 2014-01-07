using System;
using System.Collections.Generic;
using System.IO;
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
            string path = Path.GetFullPath(OutDir);
            if (!System.IO.Directory.Exists(path))
            {
                Console.WriteLine("No such directory {0}", path);
                ok = false;
            }
            else
            {
                OutDir = path;
            }
            return ok;
        }

        public override int GetHashCode()
        {
            int i = QueueName.GetHashCode();
            i = 31 * i + Exchange.GetHashCode();
            i = 31 * i + RoutingKey.GetHashCode();
            i = 31 * i + OutDir.GetHashCode();
            return i;
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
            string path = Path.GetFullPath(File);
            if (!System.IO.File.Exists(path))
            {
                Console.WriteLine("No such file {0}", path);
                ok = false;
            }
            else
            {
                File = path;
            }
            return ok;
        }
    }

}
