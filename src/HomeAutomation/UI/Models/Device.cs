using Headspring;

namespace UI.Controllers
{
    public class Device : Enumeration<Device>
    {
        public static Device Office = new Device("Office", "a14", 1, true);
        public static Device Stairway = new Device("Stairway", "a13", 2);
        public static Device DiningRm = new Device("Dining", "a12", 8, true);
        public static Device LivingRm = new Device("Living", "a11", 3);
        public static Device unknown1 = new Device("a 10", "a10", 9);
        public static Device MasterFanLight = new Device("Master Fan Light", "a9", 4, true);
        public static Device MasterFan = new Device("Master Fan", "a8", 5, true);
        public static Device MasterHisLight = new Device("Master His Light", "a7", 6, true);
        public static Device MasterHerLight = new Device("Master Her Light", "a6", 7, true);
        public static Device unknown2 = new Device("a 5", "a5", 10);
        public static Device unknown3 = new Device("a 4", "a4", 11);

        private Device(string name, string address, int id, bool dimmable = false) : base(id, name)
        {
            Address = address;
            State = "off";
            Dimmable = dimmable;
        }

        public string Address { get; set; }
        public string State { get; set; }
        public bool Dimmable { get; set; }
    }
}