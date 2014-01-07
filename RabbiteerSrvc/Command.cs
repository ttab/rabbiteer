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
    }

    [Serializable]
    public class PublishCommand : Command
    {
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public IDictionary<string, object> Headers = new Dictionary<string, object>();
        public string File { get; set; }
    }

    [Serializable]
    public class BindCommand : Command
    {
        public string QueueName { get; set; }
        public string Exchange { get; set; }
        public string RoutingKey { get; set; }
        public string OutDir { get; set; }
    }

}
