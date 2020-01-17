using System;
using ZendeskBackup.Model;
using ZendeskBackup.RestApi;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading;

namespace ZendeskBackup
{
    class Program
    {
        static void Main(string[] args)
        {
            
            ZendeskConfig config = new ZendeskConfig() { 
                Login = "",
                Password = "",
                Url = "https://support.travelline.ru"
            };
            string backupPath = @"c:\temp\backups\";
            BackupScheduler.Start(config, backupPath);
            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
        }
    }
}
