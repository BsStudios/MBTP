using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MBTP_Server
{
    class ConnectionHandler
    {
        public int threadid;
        public TcpClient client;

        private DateTime lastsend;

        NetworkStream networkStream;

        public ConnectionHandler(int threadid, TcpClient client)
        {
            this.threadid = threadid;
            this.client = client;
        }

        public void HandleClient()
        {
            networkStream = client.GetStream();
            Console.WriteLine("Connected");
            networkStream.Write(Encoding.UTF8.GetBytes("MBTP Version 1.0" + Environment.NewLine));
            lastsend = DateTime.Now;
            string connectiontype = string.Empty;
            bool isConnectionFinalized = false;
            bool isLogOn = false;
            UsersJson2 user = new UsersJson2();
            bool isWaitingForUsername = false;
            bool isWaitingForPassword = false;
            string tempusername = string.Empty;
            string temppassword = string.Empty;
            string currentpath = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                currentpath = "\\";
            }
            else
            {
                currentpath = "/";
            }
            while (client.Connected)
            {
                beginwhielloop:
                if (networkStream.DataAvailable)
                {
                    lastsend = DateTime.Now;
                    byte[] bytes = new byte[512];
                    networkStream.Read(bytes, 0, bytes.Length);
                    /*foreach (byte bit in bytes)
                    {
                        Console.Write(bit.ToString()+" ");
                    }
                    Console.WriteLine();
                    Console.WriteLine(Encoding.UTF8.GetString(bytes).TrimEscapes().ToLiteral());*/
                    if(Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "legacy" && connectiontype == string.Empty && isConnectionFinalized == false)
                    {
                        connectiontype = "legacy";
                        isConnectionFinalized = true;
                        goto beginwhielloop;
                    }
                    else if (Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "mbtp" && connectiontype == string.Empty && isConnectionFinalized == false)
                    {
                        connectiontype = "mbtp";
                        bytes = Encoding.UTF8.GetBytes("initializing MBTP Connection Stream..." + Environment.NewLine);
                        networkStream.Write(bytes, 0, bytes.Length);
                        isConnectionFinalized = true;
                        goto beginwhielloop;
                    }
                    else if (isConnectionFinalized)
                    {
                        if (connectiontype == "mbtp")
                        {
                            lastsend = DateTime.Now;
                            Console.WriteLine();
                            Console.Write(Encoding.UTF8.GetString(bytes).TrimEscapes());
                            if (Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "login plain")
                            {
                                bytes = Encoding.UTF8.GetBytes("200 Ok. Please Enter Username" + Environment.NewLine);
                                networkStream.Write(bytes, 0, bytes.Length);
                                isWaitingForUsername = true;
                                bool isActualAccount = false;
                                int accountnumber = 0;
                                while (isWaitingForUsername)
                                {
                                    if (networkStream.DataAvailable)
                                    {
                                        lastsend = DateTime.Now;
                                        bytes = new byte[512];
                                        networkStream.Read(bytes, 0, bytes.Length);
                                        isWaitingForUsername = false;
                                        tempusername = Encoding.UTF8.GetString(bytes).TrimEscapes();
                                        for(int i = 0; i < Program.uj.users.Length; i++)
                                        {
                                            if(Program.uj.users[i].username == tempusername)
                                            {
                                                isActualAccount = true;
                                                accountnumber = i;
                                                break;
                                            }
                                        }
                                    }
                                    ConnectionTimer();
                                }
                                if (isActualAccount)
                                {
                                    isWaitingForPassword = true;
                                    bytes = Encoding.UTF8.GetBytes("100 Continue. Waiting For Password" + Environment.NewLine);
                                    networkStream.Write(bytes);
                                    bool isValidPassword = false;
                                    while (isWaitingForPassword)
                                    {
                                        if (networkStream.DataAvailable)
                                        {
                                            lastsend = DateTime.Now;
                                            bytes = new byte[512];
                                            networkStream.Read(bytes, 0, bytes.Length);
                                            isWaitingForPassword = false;
                                            temppassword = Encoding.UTF8.GetString(bytes).TrimEscapes();
                                            if(Program.uj.users[accountnumber].password == temppassword)
                                            {
                                                isValidPassword = true;
                                                break;
                                            }
                                        }
                                        ConnectionTimer();
                                    }
                                    if (isValidPassword)
                                    {
                                        bytes = Encoding.UTF8.GetBytes("200 Ok. Login Success" + Environment.NewLine);
                                        networkStream.Write(bytes);
                                        isLogOn = true;
                                        user = Program.uj.users[accountnumber];
                                    }
                                    else
                                    {
                                        bytes = Encoding.UTF8.GetBytes("403 Forbiden. Invalid Login" + Environment.NewLine);
                                        networkStream.Write(bytes);
                                    }
                                }
                                else
                                {
                                    isWaitingForPassword = true;
                                    bytes = Encoding.UTF8.GetBytes("100 Continue. Waiting For Password" + Environment.NewLine);
                                    networkStream.Write(bytes);
                                    while (isWaitingForPassword)
                                    {
                                        if (networkStream.DataAvailable)
                                        {
                                            lastsend = DateTime.Now;
                                            bytes = new byte[512];
                                            networkStream.Read(bytes, 0, bytes.Length);
                                            break;
                                        }
                                        ConnectionTimer();
                                    }

                                    bytes = Encoding.UTF8.GetBytes("403 Forbiden. Invalid Login" + Environment.NewLine);
                                    networkStream.Write(bytes);
                                }
                            }
                            else if(Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower().StartsWith("dir ") || Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "dir")
                            {
                                if (isLogOn)
                                {
                                    if (Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "dir")
                                    {
                                        bytes = Encoding.UTF8.GetBytes("150 Sending Directory Listings" + Environment.NewLine);
                                        networkStream.Write(bytes);
                                        bool createddirectory = false;

                                        if (Directory.Exists(user.rootpath + currentpath))
                                        {
                                            for (int i = 0; i < Directory.GetFiles(user.rootpath + currentpath).Length; i++)
                                            {
                                                FileInfo fn = new FileInfo(Directory.GetFiles(user.rootpath + currentpath)[i]);
                                                bytes = Encoding.UTF8.GetBytes("File : " + fn.Name + " ; " + fn.Length + Environment.NewLine); ;
                                                networkStream.Write(bytes);
                                            }
                                            for (int i = 0; i < Directory.GetDirectories(user.rootpath + currentpath).Length; i++)
                                            {
                                                DirectoryInfo dn = new DirectoryInfo(Directory.GetDirectories(user.rootpath + currentpath)[i]);
                                                bytes = Encoding.UTF8.GetBytes("Directory : " + dn.Name + Environment.NewLine);
                                                networkStream.Write(bytes);
                                            }
                                            bytes = Encoding.UTF8.GetBytes("216 Directory Send OK" + Environment.NewLine);
                                            networkStream.Write(bytes);
                                        }
                                        else
                                        {
                                            if (currentpath == "\\" || currentpath == "/")
                                            {
                                                try
                                                {
                                                    Directory.CreateDirectory(user.rootpath + currentpath);
                                                    createddirectory = true;
                                                }
                                                catch (IOException io)
                                                {
                                                    createddirectory = false;
                                                }
                                                catch (UnauthorizedAccessException ua)
                                                {
                                                    createddirectory = false;
                                                }

                                                if (!createddirectory)
                                                {
                                                    bytes = Encoding.UTF8.GetBytes("550 Failed To Init Home Directory" + Environment.NewLine);
                                                    networkStream.Write(bytes);
                                                }
                                                else
                                                {
                                                    bytes = Encoding.UTF8.GetBytes("216 Directory Send OK");
                                                    networkStream.Write(bytes);
                                                }
                                            }
                                            else
                                            {
                                                bytes = Encoding.UTF8.GetBytes("550 Directory Not Found" + Environment.NewLine);
                                                networkStream.Write(bytes);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string dirpath = Encoding.UTF8.GetString(bytes).TrimEscapes().Remove(0, 4);
                                        if(user.fullaccess == "true")
                                        {

                                        }
                                        else
                                        {
                                            if (Directory.Exists(user.rootpath + "\\" + dirpath))
                                            {
                                                bytes = Encoding.UTF8.GetBytes("150 Sending Directory Listings" + Environment.NewLine);
                                                networkStream.Write(bytes);

                                                for (int i = 0; i < Directory.GetFiles(user.rootpath + "\\" + dirpath).Length; i++)
                                                {
                                                    FileInfo fn = new FileInfo(Directory.GetFiles(user.rootpath + "\\" + dirpath)[i]);
                                                    bytes = Encoding.UTF8.GetBytes("File : " + fn.Name + " ; " + fn.Length + Environment.NewLine); ;
                                                    networkStream.Write(bytes);
                                                }
                                                for (int i = 0; i < Directory.GetDirectories(user.rootpath + "\\" + dirpath).Length; i++)
                                                {
                                                    DirectoryInfo dn = new DirectoryInfo(Directory.GetDirectories(user.rootpath + "\\" + dirpath)[i]);
                                                    bytes = Encoding.UTF8.GetBytes("Directory : " + dn.Name + Environment.NewLine);
                                                    networkStream.Write(bytes);
                                                }
                                                bytes = Encoding.UTF8.GetBytes("216 Directory Send OK" + Environment.NewLine);
                                                networkStream.Write(bytes);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    bytes = Encoding.UTF8.GetBytes("530 Please Login" + Environment.NewLine);
                                    networkStream.Write(bytes);
                                }
                            }
                            else if(Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower().StartsWith("cd "))
                            {
                                if (isLogOn)
                                {
                                    if(user.fullaccess == "true")
                                    {

                                    }
                                    else
                                    {
                                        string cdpath = Encoding.UTF8.GetString(bytes).TrimEscapes().Remove(0, 3);
                                        if (cdpath.StartsWith('/') || cdpath.StartsWith('\\'))
                                        {
                                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                            {
                                                cdpath = cdpath.Replace('/', '\\');
                                            }
                                            else
                                            {
                                                cdpath = cdpath.Replace('\\', '/');
                                            }
                                            if (Directory.Exists(user.rootpath + cdpath))
                                            {
                                                currentpath = cdpath;
                                            }
                                            else
                                            {
                                                bytes = Encoding.UTF8.GetBytes("550 Directory Not Found" + Environment.NewLine);
                                                networkStream.Write(bytes);
                                            }
                                        }
                                        else
                                        {
                                            if (Directory.Exists(user.rootpath + currentpath + "\\" + cdpath) || Directory.Exists(user.rootpath + currentpath + "/" + cdpath))
                                            {
                                                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                                                {
                                                    currentpath += "\\" + cdpath;
                                                }
                                                else
                                                {
                                                    currentpath += "/" + cdpath;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    bytes = Encoding.UTF8.GetBytes("530 Please Login" + Environment.NewLine);
                                    networkStream.Write(bytes);
                                }
                            }
                            else if(Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "quit")
                            {
                                bytes = Encoding.UTF8.GetBytes("221 Bye" + Environment.NewLine);
                                networkStream.Write(bytes, 0, bytes.Length);
                                networkStream.Close();
                                client.Close();

                            }
                            else if (Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "bye")
                            {
                                bytes = Encoding.UTF8.GetBytes("221 Bye" + Environment.NewLine);
                                networkStream.Write(bytes, 0, bytes.Length);
                                networkStream.Close();
                                client.Close();
                            }
                        }
                        else if (Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "quit" && connectiontype == string.Empty)
                        {
                            bytes = Encoding.UTF8.GetBytes("221 Bye" + Environment.NewLine);
                            networkStream.Write(bytes, 0, bytes.Length);
                            networkStream.Close();
                            client.Close();

                        }
                        else if (Encoding.UTF8.GetString(bytes).TrimEscapes().ToLower() == "bye" && connectiontype == string.Empty)
                        {
                            bytes = Encoding.UTF8.GetBytes("221 Bye" + Environment.NewLine);
                            networkStream.Write(bytes, 0, bytes.Length);
                            networkStream.Close();
                            client.Close();
                        }
                    }
                }
                ConnectionTimer();
            }
            Console.WriteLine("Disconnected");
        }

        public void ConnectionTimer()
        {
            Thread.Sleep(50);
            TimeSpan offsettime = DateTime.Now.Subtract(lastsend);
            if (offsettime.Minutes >= 1)
            {
                networkStream.Close();
                client.Close();
            }
        }

    }
}
