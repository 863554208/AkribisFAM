using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AkribisFAM.CommunicationProtocol
{
    public class Task_MES
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private StreamWriter writer;
        private StreamReader reader;

        private void Connect() {
            string serverAddress = "127.0.0.1";  // 服务器地址
            int serverPort = 5012;             // 服务器端口

            try
            {
                // 创建TCP客户端并连接到服务器
                tcpClient = new TcpClient();
                tcpClient.Connect(serverAddress, serverPort);
                Console.WriteLine("Connected to the server.");

                // 获取网络流
                networkStream = tcpClient.GetStream();
                writer = new StreamWriter(networkStream, Encoding.ASCII);
                reader = new StreamReader(networkStream, Encoding.ASCII);

                // 发送一条消息
                SendMessage("Hello, Server!");

                // 接收服务器的响应
                ReceiveMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        // 发送消息到服务器
        private void SendMessage(string message)
        {
            if (tcpClient.Connected)
            {
                writer.WriteLine(message);
                writer.Flush();
                Console.WriteLine("Sent: " + message);
            }
        }

        // 接收服务器的消息
        private void ReceiveMessage()
        {
            try
            {
                string response = reader.ReadLine();
                if (response != null)
                {
                    Console.WriteLine("Received: " + response);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error receiving data: " + ex.Message);
            }
        }


}
}
