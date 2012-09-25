using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HomeAutomation
{
    public class RelayCommands:SignalR.Hubs.Hub
    {
        public void SendCommandToClient(string address, string command)
        {
            Clients.ExecuteCommand(address, command);
        }

        public void CommandSent(string address, string command)
        {
            Clients.sent(address,command);
        }
        public void CommandReceived(string address, string command)
        {
            //received a command from RF / motion sensor / remote control.
        }
        public void HeartBeat(DateTime datetime, string message)
        {
            Clients.sent(datetime.ToString(),message);
        }
    }
}