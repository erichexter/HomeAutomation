namespace LightController
{
    public class ServiceController 
    {
        X10AgentService _service = null;
        public bool Start()
        {
            _service = new X10AgentService();
            _service.Run();
            return true;
        }

        public bool Stop()
        {

            _service.Dispose();
            _service = null;
            return true;
        }
    }
}