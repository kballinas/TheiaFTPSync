using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TheiaFTPSync.Core;

namespace TheiaFTPSync.Service
{
    public partial class TheiaSyncSerive : ServiceBase
    {
        public TheiaSyncSerive()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var ftpSync = new FTPSync();
            ftpSync.RunFTPSync();
        }

        protected override void OnStop()
        {
        }
    }
}
