using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Rabbiteer
{
    public partial class RabbiteerService : ServiceBase
    {

        public Server server = new Server();

        public RabbiteerService()
        {
            InitializeComponent();
        }

        public void DoStart()
        {
            server.DoStart();
        }
        public void DoStop()
        {
            server.DoStop();
        }

        protected override void OnStart(string[] args)
        {
            DoStart();
        }

        protected override void OnStop()
        {
            DoStop();
        }
    }
}
