using System;
using System.Diagnostics;

namespace LightController
{
    public class X10Service
    {
        private Action<string> _logger = a => { };
        public X10Service()
        {
        }
        public X10Service(Action<string> logger)
        {
            _logger = logger;
        }

        public X10CommandResult SendX10Command(string command, string address)
        {
            try
            {
                var param = string.Format("sendrf {0} {1}", address, command);
                _logger(param);
                var p = System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Common Files\X10\Common\ahcmd.exe", param);
                p.WaitForExit();
                return new X10CommandResult{Success=true};
            }
            catch (Exception o_O)
            {
                _logger(o_O.Message);
                return new X10CommandResult{Success=false,Error=o_O.Message};
            }

        }
    }
    public class X10CommandResult{
        public bool Success{get;set;}
        public string Error{get;set;}
    }
}