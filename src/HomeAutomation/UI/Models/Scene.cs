using System.Collections.Generic;
using Headspring;

namespace UI.Controllers
{
    public class Scene : Enumeration<Scene>
    {
        public static Scene Dusk = new Scene(1, "Dusk")
            {
                Commands = new List<CommandState>
                    {
                        new CommandState {Command = "on", Device = Device.LivingRm},
                        new CommandState {Command = "on", Device = Device.Office},
                        new CommandState {Command = "on", Device = Device.Stairway},
                        new CommandState {Command = "on", Device = Device.MasterFanLight},
                    }
            };

        public static Scene Wakeup = new Scene(2, "Wakeup")
            {
                Commands = new List<CommandState>
                    {
                        new CommandState {Command = "on", Device = Device.LivingRm},
                        new CommandState {Command = "on", Device = Device.Office},
                        new CommandState {Command = "on", Device = Device.Stairway},
                        new CommandState {Command = "off", Device = Device.MasterFan},
                    }
            };

        public static Scene Dawn = new Scene(2, "Dawn")
            {
                Commands = new List<CommandState>
                    {
                        new CommandState {Command = "off", Device = Device.LivingRm},
                        new CommandState {Command = "off", Device = Device.Office},
                        new CommandState {Command = "off", Device = Device.Stairway},
                        new CommandState {Command = "off", Device = Device.DiningRm},
                        new CommandState {Command = "off", Device = Device.MasterFanLight},
                        new CommandState {Command = "off", Device = Device.MasterFan},
                        new CommandState {Command = "off", Device = Device.MasterHerLight},
                        new CommandState {Command = "off", Device = Device.MasterHisLight},
                    }
            };

        public static Scene Bedtime = new Scene(3, "Bedtime")
        {
            Commands = new List<CommandState>
                    {
                        new CommandState {Command = "off", Device = Device.LivingRm},
                        new CommandState {Command = "off", Device = Device.Office},
                        new CommandState {Command = "on", Device = Device.Stairway},
                        new CommandState {Command = "on", Device = Device.MasterFan},
                        new CommandState {Command = "on", Device = Device.DiningRm},
                    }
        };

        public static Scene LightsOut = new Scene(4, "Lights Out")
        {
            Commands = new List<CommandState>
                    {
                        new CommandState {Command = "off", Device = Device.LivingRm},
                        new CommandState {Command = "off", Device = Device.Office},
                        new CommandState {Command = "off", Device = Device.Stairway},
                        new CommandState {Command = "off", Device = Device.DiningRm},
                    }
        };

        public Scene(int value, string displayName) : base(value, displayName)
        {
        }

        public List<CommandState> Commands { get; set; }
    }
}