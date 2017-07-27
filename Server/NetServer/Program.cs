using System;
using System.Collections.Generic;

namespace NetServer
{
    class Program
    {
        static void Main(string[] args)
        {
            /*Socket listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 25565);
            listen.Bind(ipEndPoint);
            listen.Listen(0);
            Console.WriteLine("服务器启动");

            while (true)
            {
                Socket connection = listen.Accept();
                Console.WriteLine("【服务器】建立连接");

                byte[] readBuffer = new byte[1024];
                int count = connection.Receive(readBuffer);
                string message = Encoding.UTF8.GetString(readBuffer, 0, count);
                Console.WriteLine("【接收客户端消息】" + message);

                byte[] sendBytes = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                connection.Send(sendBytes);
            }*/

            /*Socket udp = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Broadcast, 25566);
            byte[] sendBytes = Encoding.UTF8.GetBytes("UDP Sned");
            while (true)
            {
                udp.SendTo(sendBytes, sendEndPoint);
                Console.WriteLine("UDP广播");
                Thread.Sleep(1000);
            }*/

            Server server = new Server();
            server.StartServer("0.0.0.0", 25565);

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "quit")
                    return;
            }
        }
    }
}
