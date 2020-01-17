using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using ZendeskBackup.RestApi;

namespace ZendeskBackup
{
    public class BackupCreator : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() => {
                JobDataMap dataMap = context.JobDetail.JobDataMap;
                
                ZendeskConfig zendeskConfig = new ZendeskConfig()
                {
                    Login = dataMap.GetString("login"), 
                    Password = dataMap.GetString("password"),
                    Url = dataMap.GetString("url") 
                };
                var backupFolder = dataMap.GetString("backupFolder");
                ZendeskBackup zendeskBackup = new ZendeskBackup(zendeskConfig, backupFolder);
                zendeskBackup.Run();
            });
        }
    }
}