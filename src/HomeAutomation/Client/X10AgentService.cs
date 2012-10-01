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
        internal static IHubProxy chat;
        private static X10Service x10Service;
        private IScheduler sched;
        public void Run()
        {
            var baseuri = System.Configuration.ConfigurationManager.AppSettings["base"];
            var hubConnection = new HubConnection(baseuri);

            // Create a proxy to the chat service
            chat = hubConnection.CreateProxy("relayCommands");
            x10Service = new X10Service(a => { Console.WriteLine(a); });
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
                        chat.Invoke("commandSent", "error", result.Error);
                    }

                }
                );

            chat.On("eventFired", a => { Console.WriteLine(a + " " + DateTime.Now.ToShortTimeString()); });

            chat.On("program", a => { programDevice(a); });
            // Start the connection
            hubConnection.Start().Wait();

            ISchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            sched = schedFact.GetScheduler();
            ConfigureScheduledEvents(sched);

            sched.ScheduleJob(JobBuilder.Create<HeartbeatJob>().Build(), TriggerBuilder.Create().WithSimpleSchedule(s => s.WithIntervalInSeconds(30).RepeatForever()).Build());

        }

        private static void programDevice(string a)
        {
            Console.WriteLine("Programming device " + a);
            x10Service.SendX10Command(a, "on");
            System.Threading.Thread.Sleep(1000);
            x10Service.SendX10Command(a, "on");
            System.Threading.Thread.Sleep(1000);
            x10Service.SendX10Command(a, "on");
            chat.Invoke("commandSent", a, "was programmed");
        }

        private static void ConfigureScheduledEvents(IScheduler sched)
        {
            sched.Start();


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
            sched.Shutdown(true);            
            sched = null;
            chat = null;
        }
    }
}