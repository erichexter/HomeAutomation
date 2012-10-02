using System;
using System.Collections.Generic;
using Client;
using Quartz;
using Quartz.Impl;
using SignalR.Client.Hubs;

namespace LightController
{
    public class X10AgentService:IDisposable
    {
        internal static IHubProxy hubProxy;
        private static X10Service x10Service;
        private IScheduler scheduler;
        
        public void Run()
        {
            x10Service = new X10Service(a => { Logger.Log(a); });

            hubProxy = ConfigureServerCommunications();

            Logger.loggerInternal = (message) =>
            {
                Console.WriteLine(message);
                hubProxy.Invoke("commandSent", string.Empty, message);
            };

            scheduler = ConfigureScheduledEvents();
            scheduler.Start();

        }

        private IHubProxy ConfigureServerCommunications()
        {
            var baseuri = System.Configuration.ConfigurationManager.AppSettings["base"];
            var hubConnection = new HubConnection(baseuri);


            // Create a proxy to the hubProxy service
            var chat = hubConnection.CreateProxy("relayCommands");

            // Print the message when it comes in
            chat.On("executeCommand", (string a, string b) =>
                {
                    Console.WriteLine(a + " " + b);

                    var result = x10Service.SendX10Command(b, a);
                    if (result.Success)
                    {
                        chat.Invoke("commandSent", a, b);
                    }
                    else
                    {
                        Logger.Log(result.Error);
                    }
                }
                );

            chat.On("eventFired", a => { Logger.Log(a + " " + DateTime.Now.ToShortTimeString()); });

            chat.On("program", a => { programDevice(a); });

            hubConnection.Start().Wait();
            return chat;
        }

        private IScheduler ConfigureScheduledEvents()
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            var scheduler = schedFact.GetScheduler();
            ConfigureScheduledEvents(scheduler);

            scheduler.ScheduleJob(JobBuilder.Create<HeartbeatJob>().Build(),
                              TriggerBuilder.Create().WithSimpleSchedule(s => s.WithIntervalInSeconds(30).RepeatForever()).Build
                                  ());
            return scheduler;
        }

        private void programDevice(string a)
        {
            Logger.Log("Programming device " + a);
            x10Service.SendX10Command(a, "on");
            Logger.Log("Sent 1st command");
            System.Threading.Thread.Sleep(1000);
            x10Service.SendX10Command(a, "on");
            Logger.Log("Sent 2nd command");
            System.Threading.Thread.Sleep(1000);
            x10Service.SendX10Command(a, "on");
            Logger.Log("Sent 3ed command");
            Logger.Log( a + " was programmed");
        }

        //public void Echo(string message)
        //{
        //    Console.WriteLine(message);
        //    hubProxy.Invoke("commandSent", string.Empty, message);
        //}

        private static void ConfigureScheduledEvents(IScheduler sched)
        {
            foreach (var trigger in GetTriggers())
            {
                sched.ScheduleJob(JobBuilder.Create<PublishEvent>().Build(), trigger);
            }

        }

        private static IEnumerable<ITrigger> GetTriggers()
        {

            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x => x.OnMondayThroughFriday()
                                                        .Around(new TimeOfDay(6, 0))
                )
                .UsingJobData("event", "wakeup")
                .Build();

            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x =>
                                               x.SchoolNights()
                                                   .Around(new TimeOfDay(20, 0))
                )
                .UsingJobData("event", "lightsout")
                .Build();

            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x => x.WeekendNights()
                                                        .Around(new TimeOfDay(20, 0))
                )
                .UsingJobData("event", "lightsout")
                .Build();

            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x =>
                                               x.SchoolNights()
                                                   .Around(new TimeOfDay(19, 0))
                )
                .UsingJobData("event", "bedtime")
                .Build();

            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x =>
                                               x.WeekendNights()
                                                   .Around(new TimeOfDay(20, 0))
                )
                .UsingJobData("event", "bedtime")
                .Build();
            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x =>
                                               x.OnEveryDay().Around(new TimeOfDay(17, 30))
                )
                .UsingJobData("event", "dusk")
                .Build();
            yield return TriggerBuilder
                .Create()
                .WithDailyTimeIntervalSchedule(x =>
                                               x.Around(new TimeOfDay(17, 30))
                )
                .UsingJobData("event", "dawn")
                .Build();
        }




        public void Dispose()
        {
            scheduler.Shutdown(true);            
            scheduler = null;
            hubProxy = null;
            Logger.loggerInternal = (m) => { Console.WriteLine(m); };
        }
    }
}