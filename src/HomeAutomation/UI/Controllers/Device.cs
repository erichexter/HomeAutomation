using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Controllers
{
    public class Device
    {
        public string Name { get; set; }
        public string Adress { get; set; }
    }
    public class DeviceRepository
    {
        public IList<Device> GetAll()
        {
            var model = new List<Device>();
            model.Add(new Device { Name = "Office", Adress = "a8" });
            model.Add(new Device { Name = "Stairway", Adress = "a6" });
            model.Add(new Device { Name = "Living Rm", Adress = "a9" });
            model.Add(new Device { Name = "Mastr Light", Adress = "a10" });
            model.Add(new Device { Name = "Master Fan", Adress = "a11" });
            return model;
        }

        public Device Get(string address)
        {
            return GetAll().FirstOrDefault(d=>d.Adress.Equals(address)); 
        }
    }
}
