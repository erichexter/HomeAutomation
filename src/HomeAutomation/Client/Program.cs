using Quartz;
using Quartz.Impl;
using SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Client;

namespace LightController
{
    static class Program
    {
        internal static IHubProxy chat;
        static void Main(string[] args)
        {
            var baseuri = System.Configuration.ConfigurationManager.AppSettings["base"];
            var hubConnection = new HubConnection(baseuri);

            // Create a proxy to the chat service
             chat = hubConnection.CreateProxy("relayCommands");
            var x10Service = new X10Service();
            // Print the message when it comes in
            chat.On("executeCommand", (string a,string b) => {
                Console.WriteLine(a + " " + b);
                try
                {
                    x10Service.SendX10Command(b, a);
                    chat.Invoke("commandSent", a, b);
                    
                }
                catch (Exception o_O)
                {
                    chat.Invoke("commandSent","error", o_O.Message);
                }
                }
                );

            chat.On("eventFired", a => { Console.WriteLine(a + " " + DateTime.Now.ToShortTimeString()); });

            // Start the connection
            hubConnection.Start().Wait();

            ISchedulerFactory schedFact = new StdSchedulerFactory();

            // get a scheduler
            IScheduler sched = schedFact.GetScheduler();
            ConfigureScheduledEvents(sched);

            while(true){
                Console.WriteLine("heartbeat");
                chat.Invoke("heartBeat", DateTime.Now, "x10 still running");
                System.Threading.Thread.Sleep(30000);
                
                  
            }

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
                               SchoolNights(x)
                               .Around(new TimeOfDay(20, 0))
                               )
                           .UsingJobData("event", "lightsout")
                           .Build();

            yield return TriggerBuilder
               .Create()
               .WithDailyTimeIntervalSchedule(x => WeekendNights(x)
                   .Around(new TimeOfDay(20, 0))
                   )
               .UsingJobData("event", "lightsout")
               .Build();

            yield return TriggerBuilder
               .Create()
               .WithDailyTimeIntervalSchedule(x => 
                    SchoolNights(x)
                    .Around(new TimeOfDay(19, 0))                    
                    )
               .UsingJobData("event", "bedtime")
               .Build();

            yield return TriggerBuilder
               .Create()
               .WithDailyTimeIntervalSchedule(x => 
                   WeekendNights(x)
                    .Around(new TimeOfDay(20, 0))                    
                   )
               .UsingJobData("event", "bedtime")
               .Build();
            yield return TriggerBuilder
               .Create()
               .WithDailyTimeIntervalSchedule(x =>                     
                        x.OnEveryDay().Around(new TimeOfDay(17,30))                    
                   )
               .UsingJobData("event", "dusk")
               .Build();
            yield return TriggerBuilder
               .Create()
               .WithDailyTimeIntervalSchedule(x =>
                    x.Around(new TimeOfDay(17,30))
                   )
               .UsingJobData("event", "dawn")
               .Build();
        }

        private static DailyTimeIntervalScheduleBuilder Around(this DailyTimeIntervalScheduleBuilder x,TimeOfDay time)
        {
            return x.OnEveryDay()
                .StartingDailyAt(time)
                .EndingDailyAt(new TimeOfDay(time.Hour, time.Minute+5));
        }

        private static DailyTimeIntervalScheduleBuilder SchoolNights(this DailyTimeIntervalScheduleBuilder x)
        {
            return x.OnDaysOfTheWeek(DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday);
        }

        private static DailyTimeIntervalScheduleBuilder WeekendNights(this DailyTimeIntervalScheduleBuilder x)
        {
            return x.OnDaysOfTheWeek(DayOfWeek.Friday,DayOfWeek.Saturday);
        }
    }

 

}