using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FloraCSharp.Services
{
    public class FloraDebugLogger
    {
        public void Log(string logmessage, string module)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string type = "debug";
            Console.WriteLine($"{DateTime.Now,-19} [{type,8}] {module}: {logmessage}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
