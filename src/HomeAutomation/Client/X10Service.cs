using System;

namespace LightController
{
    public class X10Service
    {
        public void SendX10Command(string command, string address)
        {
                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Common Files\X10\Common\ahcmd.exe", string.Format("sendrf {0} {1}", address, command));
 
        }

    }
}