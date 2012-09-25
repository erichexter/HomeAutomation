using SignalR.Client.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightController
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseuri = System.Configuration.ConfigurationManager.AppSettings["base"];
            var hubConnection = new HubConnection(baseuri);

            // Create a proxy to the chat service
            var chat = hubConnection.CreateProxy("relayCommands");
            var x10Service = new X10Service();
            // Print the message when it comes in
            chat.On("executeCommand", (string a,string b) => {
                Console.WriteLine(a + " " + b);
                try
                {
                    x10Service.SendX10Command(b, a);
                    chat.Invoke("commandSent", a, b);
                }
                catch (Exception o_O)
                {
                    chat.Invoke("commandSent","error", o_O.Message);
                }
                }
                );

            // Start the connection
            hubConnection.Start().Wait();

            while(true){
                Console.WriteLine("heartbeat");
                chat.Invoke("heartBeat", DateTime.Now, "x10 still running");
                System.Threading.Thread.Sleep(30000);
                
                  
            }

        }
    }

 

}