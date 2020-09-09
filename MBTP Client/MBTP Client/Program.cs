using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MBTP_Client
{
    class Program
    {
        public static TcpClient tcpClient = new TcpClient();
        public static bool isRunning = true;
        public static NetworkStream networkStream;

        public static Thread sendthread = new Thread(sendthreadfunc);
        public static Thread receivethread = new Thread(receivethreeadfunc);

        static void Main(string[] args)
        {
            tcpClient.Connect("localhost", 2121);
            networkStream = tcpClient.GetStream();
            sendthread.Start();
            receivethread.Start();
            while (isRunning)
            {
                if (!tcpClient.Connected)
                {
                    isRunning = false;
                }
                Thread.Sleep(50);
            }
            sendthread.Join();
            receivethread.Join();
            Environment.Exit(0);
        }

        public static void sendthreadfunc()
        {
            while (isRunning)
            {
                string input = Console.ReadLine();
                byte[] data;
                data = Encoding.UTF8.GetBytes(input);
                networkStream.Write(data, 0, data.Length);
                Thread.Sleep(10);
            }
        }
        public static void receivethreeadfunc()
        {
            while (isRunning)
            {
                if(tcpClient.Available > 0)
                {
                    byte[] data = new byte[512];
                    networkStream.Read(data, 0, data.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(data));
                }
                else
                {
                    Thread.Sleep(50);
                }
                Thread.Sleep(5);
            }
        }
    }
}
