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
           //Logger.Log("heartbeat " + DateTime.Now.ToShortTimeString());            
            X10AgentService.hubProxy.Invoke("heartBeat", DateTime.Now.ToShortTimeString());
        }
    }
}
