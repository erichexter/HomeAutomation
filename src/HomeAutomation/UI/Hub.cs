using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UI.Controllers;

namespace HomeAutomation
{
    public class RelayCommands:SignalR.Hubs.Hub
    {
        DeviceRepository repository = new DeviceRepository();
        public void SendCommandToClient(string address, string command)
        {
            Clients.ExecuteCommand(address, command);
        }

        public void TimedEvent(string eventname)
        {
            Clients.eventFired(eventname);
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
    }
}