using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UI.Controllers;

namespace HomeAutomation
{
    public class RelayCommands:SignalR.Hubs.Hub
    {
        DeviceRepository repository = new DeviceRepository();
        SceneRepository sceneRepository=new SceneRepository();
        public void SendCommandToClient(string address, string command)
        {
            Clients.ExecuteCommand(address, command);
        }

        public void TimedEvent(string eventname)
        {
            Clients.eventFired(eventname);
        }

        public void programNewDevice(string address)
        {
            Clients.program(address);
        }
        public void CommandSent(string address, string command)
        {
            var device = repository.Get(address);
            if (device != null)
            {
                device.State = command;
            }
            var id = device!=null?device.DisplayName:address;
            Clients.sent(id,command);
        }
        public void CommandReceived(string address, string command)
        {
            //received a command from RF / motion sensor / remote control.
        }
        public void HeartBeat(DateTime datetime, string message)
        {
            Clients.heartBeatReceived(datetime.ToString(),message);
        }

        public void RunScene(int sceneId)
        {
            var scene = sceneRepository.Get(sceneId);
            var js = Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings());
            StringWriter jw = new StringWriter();
            js.Serialize(jw,scene);
            Clients.runSceneOnAgent(jw.ToString());
        }
    }
}