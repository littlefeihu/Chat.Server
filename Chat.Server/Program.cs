using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Server
{
    class Program
    {
        const string ServerURI = "http://localhost:8080";

        static void Main(string[] args)
        {
            try
            {
                WebApp.Start(ServerURI);
            }
            catch (TargetInvocationException)
            {
                Console.WriteLine("A server is already running at " + ServerURI);
                return;
            }

            Console.WriteLine("Server started at " + ServerURI);
            Console.ReadKey();
        }
    }
}
