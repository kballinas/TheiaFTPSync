using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheiaFTPSync.Core
{
    public class FTPSync
    {
        public void RunFTPSync()
        {
            var now = DateTime.UtcNow.ToLocalTime();
            var lastRun = ReadLastRun();
            using (SftpClient ftp = new SftpClient(ConfigurationManager.AppSettings["FTP_URL"], 
                ConfigurationManager.AppSettings["FTP_Username"], 
                ConfigurationManager.AppSettings["FTP_Password"]))
            {
                var ftp_directory = ConfigurationManager.AppSettings["FTP_Directory"];
                ftp.Connect();
                var files = ftp.ListDirectory(ftp_directory);
                ftp_directory.Split('/').ToList().ForEach(f => ftp.ChangeDirectory(f));

                files=files.Where(f => f.Attributes.LastWriteTime > lastRun && f.Attributes.LastWriteTime < now);
                //WriteLastRan(now.ToString()); //uncommentn after done testing
            }
        }

        private DateTime ReadLastRun()
        {
            DateTime? rtn = null;
            using (var lastRanStream = File.OpenRead(string.Format("{0}\\{1}", ConfigurationManager.AppSettings["Local_Directory"], "last ran.txt")))
            {
                using (var lastRanReader = new StreamReader(lastRanStream))
                {
                    rtn=DateTime.Parse(lastRanReader.ReadToEnd());
                }
            }
            return rtn.Value;
        }

        private void WriteLastRan(string text)
        {
            using (var lastRanStream = File.OpenWrite(string.Format("{0}\\{1}", ConfigurationManager.AppSettings["Local_Directory"], "last ran.txt")))
            {
                using (var lastRanWriter = new StreamWriter(lastRanStream))
                {
                    lastRanWriter.WriteLine(text);
                    lastRanWriter.Flush();
                }
            }
        }
    }
}
