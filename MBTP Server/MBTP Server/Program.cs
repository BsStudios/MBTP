using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MBTP_Server
{
    class Program
    {
        public static UsersJson uj;
        public static ConfigJson cj;

        public static List<ConnectionHandler> connectionHandlers = new List<ConnectionHandler>();
        public static List<Thread> connectionthreads = new List<Thread>();

        public static bool isRunning = true;

        static void Main(string[] args)
        {
            uj = JsonConvert.DeserializeObject<UsersJson>(File.ReadAllText(".\\resources\\users.json"));
            cj = JsonConvert.DeserializeObject<ConfigJson>(File.ReadAllText(".\\resources\\config.json"));

            TcpListener listener;
            if(cj.port == string.Empty)
            {
                cj.port = "2121";
            }
            if(cj.ipaddress == "*")
            {
                listener = new TcpListener(IPAddress.Any, int.Parse(cj.port));
            }
            else
            {
                listener = new TcpListener(IPAddress.Parse(cj.ipaddress), int.Parse(cj.port));
            }
            listener.Start();
            while(isRunning == true)
            {
                Console.WriteLine("Listening For Connection");
                TcpClient tempclient = listener.AcceptTcpClient();
                Console.WriteLine("Configuring Connection");
                connectionHandlers.Add(new ConnectionHandler(connectionthreads.Count, tempclient));
                connectionthreads.Add(new Thread(connectionHandlers[connectionthreads.Count].HandleClient));
                connectionthreads[connectionthreads.Count - 1].Start();
            }
        }
    }
}
