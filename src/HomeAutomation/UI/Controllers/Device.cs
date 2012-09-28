using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Controllers
{
    public class Device:Headspring.Enumeration<Device>
    {
        Device(string name,string address,int id): base(id,name)
        {
            Address = address;
            State = "off";
        }
        public string Address { get; set; }
        public string State { get; set; }

        public static Device Office         = new Device("Office", "a8",1);
        public static Device Stairway       = new Device("Stairway","a6",2);
        public static Device LivingRm       = new Device("Living Rm","a9",3);
        public static Device MasterFanLight = new Device("Master Fan Light","a10",4);
        public static Device MasterFan      = new Device("Master Fan","a11",5);
        public static Device MasterHisLight = new Device("Master His Light","a12",6);
        public static Device MasterHerLight = new Device("Master Her Light","a13" ,7);
    }

    public class DeviceRepository
    {
        public IList<Device> GetAll()
        {
            return Headspring.Enumeration<Device>.GetAll().ToList();
        }

        public Device Get(string address)
        {
            return GetAll().FirstOrDefault(d=>d.Address.Equals(address)); 
        }
    }
}
