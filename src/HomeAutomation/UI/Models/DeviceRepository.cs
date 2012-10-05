using System.Collections.Generic;
using System.Linq;
using Headspring;

namespace UI.Controllers
{
    public class DeviceRepository
    {
        public IList<Device> GetAll()
        {
            return Enumeration<Device>.GetAll().ToList();
        }

        public Device Get(string address)
        {
            return GetAll().FirstOrDefault(d => d.Address.Equals(address));
        }
    }
}