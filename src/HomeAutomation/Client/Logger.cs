using System;

namespace LightController
{
    public static class Logger
    {
        public static Action<string> loggerInternal = (a) =>{};

        public static void Log(string Message)
        {
            loggerInternal(Message);
        }
    }
}