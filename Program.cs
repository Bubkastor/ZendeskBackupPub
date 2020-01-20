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
            var login = "";
            var password = "";
            var backupPath = @"c:\temp\backups\";
            var runNow = false;

            for(var i = 0; i < args.Length; i = i + 2)
            {
                if(args[i] == "-l")
                {
                    login = (i + 1) < args.Length ? args[i + 1] : login;
                    continue;
                }
                if (args[i] == "-p")
                {
                    password = (i + 1) < args.Length ? args[i + 1]: password;
                    continue;
                }
                if (args[i] == "-b")
                {
                    backupPath = (i + 1) < args.Length?  args[i + 1]: backupPath;
                    continue;
                }
                if (args[i] == "-run")
                {
                    runNow = true;
                    continue;
                }

            }
            Console.WriteLine($"Login: {login}");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Backup path: {backupPath}");
            Console.WriteLine($"Run now: {runNow}");
            if(login == "" && password == "")
            {
                Console.WriteLine("Please run programm with argument");
                Console.WriteLine("-l login -p password -b backupPath -run(create backup right now)");
                return;
            }

            ZendeskConfig config = new ZendeskConfig() { 
                Login = login,
                Password = password,
                Url = "https://support.travelline.ru"
            };            
            BackupScheduler.Start(config, backupPath, runNow);
            while (true)
            {
                Thread.Sleep(int.MaxValue);
            }
            
        }
    }
}
