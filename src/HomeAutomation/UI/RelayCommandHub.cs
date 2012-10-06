using System;
using System.IO;
using System.Linq;
using Loggly;
using Newtonsoft.Json;
using SignalR.Hubs;
using UI.Controllers;
using UI.Models;

namespace HomeAutomation
{
    public class RelayCommandHub : Hub
    {
        private readonly DeviceRepository _deviceRepository = new DeviceRepository();
        private readonly SceneRepository _sceneRepository = new SceneRepository();
        private Loggly.ILogger _ = new Logger("155db123-359e-4461-a836-d17d265bc2a1");
 
        public void SendCommandToClient(string address, string command)
        {
            Log("send command to client "+ address + " " + command);
            Clients.ExecuteCommand(address, command);
        }

        public void TimedEvent(string eventname)
        {
            Log("timed event "+eventname);
            Scene scene =
                _sceneRepository.GetAll().FirstOrDefault(
                    e => e.DisplayName.Replace(" ","").Equals(eventname, StringComparison.InvariantCultureIgnoreCase));
            if (scene != null)
            {
                RunScene(scene.Value);
                Clients.eventFired(eventname);
            }
        }

        public void programNewDevice(string address)
        {
            Log("program " + address);
            Clients.program(address);
        }

        private void Log(string message)
        {
            _.Log(message);
        }

        public void CommandSent(string address, string command)
        {
            Log("command sent "+ address+" " + command);
            var device = _deviceRepository.Get(address);
            if (device != null)
            {
                device.State = command;
            }
            string id = device != null ? device.DisplayName : address;
            Clients.sent(id, command);
        }

        public void CommandReceived(string address, string command)
        {
            //received a command from RF / motion sensor / remote control.
        }

        public void HeartBeat(DateTime datetime, string message)
        {
            Log("heartbeat");
            Clients.heartBeatReceived(datetime.ToString(), message);
        }

        public void RunScene(int sceneId)
        {
            var scene = _sceneRepository.Get(sceneId);
            Log("run scene "+scene.DisplayName);
            Clients.runSceneOnAgent(scene.ToJson());
        }
    }
}