using LightController;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    public class HeartbeatJob:IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("heartbeat");
            X10AgentService.chat.Invoke("heartBeat", DateTime.Now, "x10 still running");
        }
    }
}
