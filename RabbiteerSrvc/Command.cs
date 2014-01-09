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
                Log.Error("Missing QueueName -q");
                ok = false;
            }
            if (Exchange == null)
            {
                Log.Error("Missing Exchange -e");
                ok = false;
            }
            if (RoutingKey == null)
            {
                Log.Error("Missing RoutingKey -r");
                ok = false;
            }
            if (OutDir == null)
            {
                Log.Error("Missing OutDir -o");
                ok = false;
            }
            else
            {
                string path = Path.GetFullPath(OutDir);
                if (!System.IO.Directory.Exists(path))
                {
                    Log.Warn("Warning, directory does not exist: {0}", path);
                    // this is not a stop condition
                }
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
        public String ReadyDir { get; set; }

        public override bool Validate()
        {
            bool ok = true;
            if (Exchange == null)
            {
                Log.Error("Missing Exchange -e");
                ok = false;
            }
            if (RoutingKey == null)
            {
                Log.Error("Missing RoutingKey -r");
                ok = false;
            }
            if (File == null)
            {
                Log.Error("Missing File -f");
                ok = false;
            }
            else
            {
                string path = Path.GetFullPath(File);
                if (!System.IO.File.Exists(path))
                {
                    Log.Error("No such file {0}", path);
                    ok = false;
                }
                else
                {
                    File = path;
                }
            }
            if (ReadyDir == null)
            {
                Log.Error("Missing ReadyDir -d");
                ok = false;
            }
            else
            {
                string path = Path.GetFullPath(ReadyDir);
                if (!System.IO.Directory.Exists(path))
                {
                    Log.Warn("Warning, directory does not exist: {0}", path);
                    // this is not a stop condition
                }
                ReadyDir = path;
            }
            return ok;
        }

        public override int GetHashCode()
        {
            int i = Exchange.GetHashCode();
            i = 31 * i + RoutingKey.GetHashCode();
            i = 31 * i + File.GetHashCode();
            i = 31 * i + ReadyDir.GetHashCode();
            return i;
        }

    }

}
