using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheiaFTPSync.Core;

namespace TheiaFTPSync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ftpSync = new FTPSync();
            ftpSync.RunFTPSync();
            Console.ReadKey();
        }
    }
}
