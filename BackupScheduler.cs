﻿using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Text;
using ZendeskBackup.RestApi;
namespace ZendeskBackup
{
    public class BackupScheduler
    {
        public static async void Start(ZendeskConfig config, string backupFolder)
        {
            int initHour = 1;
            int initMinutes = 1;
            int initSeconds = 1;

            DateTime dateNow = DateTime.Now;
            DateTimeOffset dateNightOffset = new DateTimeOffset(new DateTime(
                dateNow.Year,
                dateNow.Month,
                dateNow.AddDays(1).Day,
                initHour,
                initMinutes,
                initSeconds));

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<BackupCreator>()
                .UsingJobData("login", config.Login)
                .UsingJobData("password", config.Password)
                .UsingJobData("url", config.Url)
                .UsingJobData("backupFolder", backupFolder)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                //.StartAt(dateNightOffset)
                .StartNow()
                .WithSimpleSchedule(x => x
                    //.WithIntervalInHours(24)
                    .WithIntervalInSeconds(10)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}