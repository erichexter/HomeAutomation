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

    public class SceneRepository
    {
        public IList<Scene> GetAll()
        {
            return Enumeration<Scene>.GetAll().ToList();
        } 
        public Scene Get(int id)
        {
            return GetAll().FirstOrDefault(s => s.Value.Equals(id));
        }
    }
}