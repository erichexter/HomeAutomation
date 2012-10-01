using System;
using System.Linq;
using System.Text;

namespace LightController
{
    static class Program
    {
        static void Main(string[] args)
        {
            using (var service = new X10AgentService())
            {
                service.Run();
                Console.WriteLine("Press any key to shutdown.");
                Console.ReadKey();    
            }
            
        }
    }
}