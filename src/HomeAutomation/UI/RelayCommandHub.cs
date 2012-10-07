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
        private DateTime _lastHeartbeat;

        public void RequestDeviceStateChange(string address, string state)
        {
            Log("send state to client "+ address + " " + state);
            //issue the command
            Clients.requestDeviceStateChangeOnAgent(address, state);
            
            //tell the clients this happened.. assume it will.. the change will revert if the update fails.
            var device = _deviceRepository.Get(address);
            if (device != null)
            {
                device.State = state;
                Clients.broadcastDeviceState(device.Value.ToString(), state);
            }
        }

        public void FireTimedEvent(string eventname)
        {
            Log("timed event "+eventname);
            Scene scene =
                _sceneRepository.GetAll().FirstOrDefault(
                    e => e.DisplayName.Replace(" ","").Equals(eventname, StringComparison.InvariantCultureIgnoreCase));
            if (scene != null)
            {
                RequestSceneExecution(scene.Value);
                Clients.broadcastEventFired(eventname);
            }
        }

        public void RequestProgramNewDevice(string address)
        {
            Log("program " + address);
            Clients.requestProgramNewDeviceOnAgent(address);
        }

        private void Log(string message)
        {
            _.Log(message);
        }

        public void DeviceStateChanged(string address, string state)
        {
            Log("state sent "+ address+" " + state);
            var device = _deviceRepository.Get(address);
            if (device != null)
            {
                device.State = state;
                Clients.broadcastDeviceState(device.Value.ToString(), state);
            }
        }

        public void HeartBeat(DateTime datetime)
        {
            _lastHeartbeat = datetime;
            Log("heartbeat");
            Clients.broadcastHeartbeat(datetime.ToString());
        }

        public void RequestSceneExecution(int sceneId)
        {
            var scene = _sceneRepository.Get(sceneId);
            Log("run scene "+scene.DisplayName);
            Clients.requestSceneExecutionOnAgent(scene.ToJson());
        }
    }
}