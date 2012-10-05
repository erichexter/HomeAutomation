using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using SignalR.Hubs;
using UI.Controllers;

namespace HomeAutomation
{
    public class RelayCommands : Hub
    {
        private readonly DeviceRepository repository = new DeviceRepository();
        private readonly SceneRepository sceneRepository = new SceneRepository();

        public void SendCommandToClient(string address, string command)
        {
            Clients.ExecuteCommand(address, command);
        }

        public void TimedEvent(string eventname)
        {
            Scene scene =
                sceneRepository.GetAll().FirstOrDefault(
                    e => e.DisplayName.Replace(" ","").Equals(eventname, StringComparison.InvariantCultureIgnoreCase));
            if (scene != null)
            {
                RunScene(scene.Value);
                Clients.eventFired(eventname);
            }
        }

        public void programNewDevice(string address)
        {
            Clients.program(address);
        }

        public void CommandSent(string address, string command)
        {
            Device device = repository.Get(address);
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
            Clients.heartBeatReceived(datetime.ToString(), message);
        }

        public void RunScene(int sceneId)
        {
            Scene scene = sceneRepository.Get(sceneId);
            JsonSerializer js = JsonSerializer.Create(new JsonSerializerSettings());
            var jw = new StringWriter();
            js.Serialize(jw, scene);
            Clients.runSceneOnAgent(jw.ToString());
        }
    }
}