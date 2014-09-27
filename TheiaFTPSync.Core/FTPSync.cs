using Renci.SshNet;
using Renci.SshNet.Sftp;
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
                ftp_directory.Split('/').ToList().ForEach(f => ftp.ChangeDirectory(f));
                var files = ftp.ListDirectory(".");

                files = files.Where(f => !f.Name.Equals(".") && !f.Name.Equals("..")).Where(f => f.LastWriteTime > lastRun && f.LastWriteTime < now).Where(f =>
                {
                    var actualTime = f.ActualLastWriteTime(ftp, ftp_directory);
                    return
                    actualTime > lastRun &&
                    actualTime < now;
                }).ToList();
                DownloadFiles(files, ftp, ftp_directory);
                WriteLastRan(now.ToString()); //uncommentn after done testing
                //Run shell script to update theia library
            }
        }

        private void DownloadFiles(IEnumerable<SftpFile> files, SftpClient ftp, string root)
        {
            foreach (var file in files)
            {
                if (file.IsDirectory)
                {
                    Directory.CreateDirectory(string.Format("{0}\\{1}", ConfigurationManager.AppSettings["Local_Directory"], file.FullName.Substring(file.FullName.IndexOf(root, 0) + root.Length)));
                    var children = ftp.ListDirectory(string.Format("./{0}", file.FullName.Substring(file.FullName.IndexOf(root, 0) + root.Length))).Where(w => !w.Name.Equals(".") && !w.Name.Equals(".."));
                    DownloadFiles(children, ftp, root);
                }
                else
                {
                    using (Stream fileStream = File.OpenWrite(string.Format("{0}\\{1}", ConfigurationManager.AppSettings["Local_Directory"], file.FullName.Substring(file.FullName.IndexOf(root, 0) + root.Length))))
                    {
                        ftp.DownloadFile(file.Name, fileStream);
                        fileStream.Flush();
                    }
                }
            }
        }

        private DateTime ReadLastRun()
        {
            DateTime? rtn = null;
            using (var lastRanStream = File.OpenRead(string.Format("{0}\\{1}", ConfigurationManager.AppSettings["Local_Directory"], "last ran.txt")))
            {
                using (var lastRanReader = new StreamReader(lastRanStream))
                {
                    rtn = DateTime.Parse(lastRanReader.ReadToEnd());
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
                }
            }
        }
    }

    public static class Extensions
    {
        public static DateTime ActualLastWriteTime(this SftpFile file, SftpClient ftp, string root)
        {
            var rtn = file.Attributes.LastWriteTime;

            if (file.Attributes.IsDirectory)
            {
                var children = ftp.ListDirectory(string.Format("./{0}", file.FullName.Substring(file.FullName.IndexOf(root, 0) + root.Length)))
                    .Where(w => !w.Name.Equals(".") && !w.Name.Equals(".."));
                var lastWrittenFile = children
                    .OrderBy(o => o.ActualLastWriteTime(ftp, root)).LastOrDefault();
                if (lastWrittenFile != null)
                {
                    rtn = lastWrittenFile.ActualLastWriteTime(ftp, root);
                }
            }

            return rtn;
        }
    }
}
