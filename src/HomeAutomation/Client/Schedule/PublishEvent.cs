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
            string e = context.MergedJobDataMap["event"].ToString();
            X10AgentService.hubProxy.Invoke("timedEvent", e); ;
            Logger.Log("Fired event " + e);
        }
    }
}
