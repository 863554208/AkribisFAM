using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json.Linq;
//using AkribisFAM.Util;

namespace AkribisFAM.CommunicationProtocol
{
    public enum ClientNames
    {
        
        camera1_Feed,
        
        camera1_Runner,
        
        camera2,
        
        camera3,
        
        lazer,
        
        scanner,
        
        mes,

        ModbusTCP,

        Pressure_sensor
    }

    class TCPNetworkManage
    {
        // 添加一个新的映射表：枚举 => (IP, Port)
        public static ConcurrentDictionary<ClientNames, (string ip, int port)> clientNameToEndpoint = new ConcurrentDictionary<ClientNames, (string, int)>();
        // 用来存储每个客户端的连接，字典的键是ClientNames，值是 TcpClientWorker 实例
        public static ConcurrentDictionary<ClientNames, TcpClientWorker> namedClients = new ConcurrentDictionary<ClientNames, TcpClientWorker>();

        /// <summary>
        /// 程序初始化加载IP地址配置文件,并连接所有客户端
        /// </summary>
        public static void TCPInitialize()
        {
            ConnectAllClients();//连接所有客户端
        }

        /// <summary>
        /// 加载所有客户端的IP地址和端口号
        /// </summary>
        private static void Readdevicesjson()
        {
            try
            {
                string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "devices.json");// 获取文件路径
                string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
                JObject obj = JObject.Parse(json);
                clientNameToEndpoint.TryAdd(ClientNames.camera1_Feed, ((string ip, int port))((obj["camera1_Feed"]["IP"]).ToString(), (obj["camera1_Feed"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.camera1_Runner, ((string ip, int port))((obj["camera1_Runner"]["IP"]).ToString(), (obj["camera1_Runner"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.camera2, ((string ip, int port))((obj["camera2"]["IP"]).ToString(), (obj["camera2"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.camera3, ((string ip, int port))((obj["camera3"]["IP"]).ToString(), (obj["camera3"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.lazer, ((string ip, int port))((obj["lazer"]["IP"]).ToString(), (obj["lazer"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.scanner, ((string ip, int port))((obj["scanner"]["IP"]).ToString(), (obj["scanner"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.mes, ((string ip, int port))((obj["mes"]["IP"]).ToString(), (obj["mes"]["Port"])));
                clientNameToEndpoint.TryAdd(ClientNames.ModbusTCP, ((string ip, int port))((obj["ModbusTCP"]["IP"]).ToString(), (obj["ModbusTCP"]["Port"])));
                //clientNameToEndpoint.TryAdd(ClientNames.Pressure_sensor, ((string ip, int port))((obj["Pressure_sensor"]["IP"]).ToString(), (obj["Pressure_sensor"]["Port"])));
                // 其他客户端可以继续添加
            }
            catch (Exception ex)
            {
                MessageBox.Show("Read device IP failed!");
            }
        }
        /// <summary>
        /// 加载单独客户端的IP地址和端口号
        /// </summary>
        private static void Readdevicesjson(ClientNames clientName)
        {
            string filePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "devices.json");// 获取文件路径
            string json = File.ReadAllText(filePath);// 读取JSON文件并反序列化为对象
            JObject obj = JObject.Parse(json);

            if (clientNameToEndpoint.ContainsKey(clientName))
            {
                clientNameToEndpoint[clientName] = ((string ip, int port))((obj[clientName.ToString()]["IP"]).ToString(), (obj[clientName.ToString()]["Port"]));
            }
            else
            {
                clientNameToEndpoint.TryAdd(clientName, ((string ip, int port))((obj[clientName.ToString()]["IP"]).ToString(), (obj[clientName.ToString()]["Port"])));
            }
        }

        /// <summary>
        /// 连接所有客户端
        /// </summary>
        public static void ConnectAllClients()
        {
            Task.Run(new Action(() =>
            {
                Readdevicesjson(); //加载所有客户端的IP地址和端口号

                // 存储连接任务的列表，Task 用于异步连接所有客户端
                List<Task> connectTasks = new List<Task>();
                // 遍历目标配置，为每个IP和端口创建一个TcpClientWorker并开始连接
                foreach (var pair in clientNameToEndpoint)
                {
                    var clientName = pair.Key;
                    var (ip, port) = pair.Value;
                    var worker = new TcpClientWorker(ip, port);// 创建新的TcpClientWorker实例
                    namedClients[clientName] = worker; // // 将TcpClientWorker添加到字典中,同时保存枚举对应的 worker
                    connectTasks.Add(worker.ConnectAsync());// 启动连接任务
                }
                // 输出正在等待所有客户端连接的信息
                //Console.WriteLine("Waiting for all clients to connect...");
                //Logger.WriteLog("Waiting for all clients to connect...");
                // 等待所有连接完成，如果任何一个连接还没完成，程序会在这里阻塞直到所有连接都成功
                Task.WhenAll(connectTasks).Wait();




                // 所有客户端连接完成后，输出成功信息
                //Console.WriteLine("All clients connected!");
               
            }
            ));
        }

        /// <summary>
        /// 连接单独的客户端
        /// </summary>
        public static void ConnectClient(ClientNames clientName)
        {
            Task.Run(new Action(() =>
            {
                Readdevicesjson(clientName);//加载单独客户端的IP地址和端口号
                if (!namedClients.ContainsKey(clientName))  // 检查字典中是否存在这个键的客户端连接
                {
                    var (ip, port) = clientNameToEndpoint[clientName];
                    var worker = new TcpClientWorker(ip, port);// 创建新的TcpClientWorker实例
                    namedClients[clientName] = worker;
                    // 输出正在等待当前客户端连接的信息
                    //    Console.WriteLine("Waiting for all clients to connect...");
                    worker.ConnectAsync().Wait();// 等待这个客户端的连接
                                                 // 当前客户端连接完成后，输出成功信息
                                                 //  Console.WriteLine("All clients connected!");
                }
                else {
                    namedClients[clientName].ConnectAsync().Wait();
                }
            }
            ));
        }

        /// <summary>
        /// 停止所有客户端的连接
        /// </summary>
        public static void StopAllClients()
        {
            if (namedClients != null & namedClients.Count > 0)
            {
                // 退出时停止所有客户端
                foreach (var client in namedClients)
                {
                    var clientname = client.Key;
                    var clientworker = client.Value;
                    clientworker.Stop();  // 停止每个客户端的连接
                   // Logger.WriteLog($"Stop {clientname.ToString()}connection");
                }
                //namedClients.Clear();
            }
        }


        /// <summary>
        /// 停止单独客户端的连接
        /// </summary>
        public static void StopClient(ClientNames clientName)
        {
            if (namedClients.ContainsKey(clientName))  // 检查字典中是否存在这个客户端连接
            {
                namedClients[clientName].Stop();  // 停止单独客户端的连接
                //Logger.WriteLog($"Stop {clientName.ToString()}connection");
                //namedClients.TryRemove(clientName, out _);
            }
        }


        /// <summary>
        /// 接收用户输入并转发到相应客户端
        /// </summary>
        public static void InputLoop(ClientNames clientName, string message)
        {
            if (string.IsNullOrEmpty(message)) return;// 如果输入为空则退出
            try
            {
                if (namedClients.ContainsKey(clientName))  // 检查字典中是否存在这个客户端连接
                {
                    namedClients[clientName].Send(message);  // 发送消息
                }
                else
                {
                    //Console.WriteLine("Client not found for given IP and port.");  // 如果找不到客户端，输出错误信息
                    //Logger.WriteLog("Client not found for given IP and port.");
                }
            }
            catch (Exception e) 
            { 
                Debug.WriteLine("tcp服务器发送数据出错 : {0}",e.ToString());
            }

        }

        //清除指定客户端的最近一条消息
        public static void ClearLastMessage(ClientNames clientName)
        {
            if (namedClients.TryGetValue(clientName, out var worker))
            {
                worker.StrClear();
            }  
        }

        // 获取指定客户端的最近一条消息
        public static string GetLastMessage(ClientNames clientName)
        {
            if (namedClients.TryGetValue(clientName, out var worker))
            {
                return worker.LastReceivedMessage ?? string.Empty;
            }
            return string.Empty;
        }

        // 获取指定客户端的所有缓存消息
        public static IEnumerable<string> GetAllMessages(ClientNames clientName)
        {
            if (namedClients.TryGetValue(clientName, out var worker))
            {
                return worker.GetCachedMessages();
            }
            return new List<string>();
        }
    }
}