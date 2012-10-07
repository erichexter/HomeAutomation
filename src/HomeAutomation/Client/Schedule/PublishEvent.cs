using LightController;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public class PublishEvent:IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            string eventName = context.MergedJobDataMap["event"].ToString();
            X10AgentService.hubProxy.Invoke("fireTimedEvent", eventName); ;
            Logger.Log("Fired event " + eventName);
        }
    }
}
