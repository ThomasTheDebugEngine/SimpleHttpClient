using System;

namespace SimpleHttpClient
{
    class Program
    {    
        static void Main(string[] args)
        {
            Server server = new Server(new Hypertext());
            server.StartServer();
        }
    }
}
