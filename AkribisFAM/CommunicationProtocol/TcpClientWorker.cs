using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using AkribisFAM.Util;

namespace AkribisFAM.CommunicationProtocol
{
    // TcpClientWorker 类用于管理每个TCP客户端的连接、消息收发和重连机制
    class TcpClientWorker
    {
        private readonly string host;  // 存储服务器的IP地址
        private readonly int port;    // 存储服务器的端口
        private Socket socket = null;        // 用来进行TCP连接的Socket对象
        private readonly object socketLock = new object();  // 用于锁定socket，防止并发访问
        private volatile bool isRunning = true;  //控制线程运行,控制客户端是否继续运行
        public volatile bool isConnected;
       // private 






        // 构造函数，初始化客户端连接信息
        public TcpClientWorker(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        // 异步连接到目标服务器
        public Task ConnectAsync()
        {
            return Task.Run(new Action(() =>
            {
                int retryCount = 0;
                const int maxRetries = 5;//重新连接最大次数
                while (true)
                {
                    if (retryCount == maxRetries || retryCount > maxRetries)
                    {
                       // Logger.WriteLog($"{maxRetries} connection failures, exit the loop!");//多次连接失败，跳出循环
                        //Console.WriteLine($"{maxRetries} connection failures, exit the loop!");
                        break;  // 多次连接失败，跳出循环
                    }
                    Socket tempSocket = null;
                    try
                    {
                        tempSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// 创建新的Socket连接
                        tempSocket.Connect(new IPEndPoint(IPAddress.Parse(host), port));  // 连接到指定IP和端口,如果抛异常，则不会执行下一步
                      //  Logger.WriteLog($"[{host}:{port}] Connected to {host}!");//该服务器连接成功
                        //Console.WriteLine($"[{host}:{port}] Connected to {host}!");
                        socket = tempSocket;  // 替换成员变量,确保连接成功后才赋值
                        Task.Run(() => ReceiveLoop());  // 启动接收消息的循环
                        isConnected = tempSocket.Connected;
                        break;  // 连接成功，跳出循环
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                       // Logger.WriteLog($"[{host}:{port}] Connection failed: {ex.Message}, {retryCount} retry after 2 seconds");//端口号连接失败，第一次重试
                        //Console.WriteLine($"[{host}:{port}] Connection failed: {ex.Message}, {retryCount} retry after 2 seconds");
                        tempSocket?.Dispose();  // 确保释放失败的 socket
                        Thread.Sleep(30);  // 如果连接失败，等待2秒后重试
                        isConnected = tempSocket.Connected;
                    }
                }
            }
            ));
        }

        public string LastReceivedMessage { get; private set; } = null;
        private readonly object LastReceivedMessageLock = new object();
        private ConcurrentQueue<string> messageCache= new ConcurrentQueue<string>();// 消息缓存结构为 ConcurrentQueue：用于先进先出方式获取最新消息
        private const int MaxCacheSize = 10; // 最大缓存大小，超出时删除最旧的消息（在添加时控制）


        // 消息接收循环
        private void ReceiveLoop()
        {
           
            byte[] buffer = new byte[4096];  // 接收消息的缓冲区
            try
            {
                while (isRunning && socket != null && socket.Connected)
                {
                    int len = socket.Receive(buffer);  // 接收消息,阻塞当前线程,直到有数据到来或连接断开,异常，程序才会继续往下执行
                    if (len > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, len);  // 将接收到的字节转换为字符串 
                        lock (LastReceivedMessageLock)
                        {
                            LastReceivedMessage = message;//保存最近接收到的消息
                        }
                        AddToCache(message);//将接收到的消息添加到缓存队列
                        //Logger.WriteLog($"[{host}:{port}] Received: {message}");//该端口号收到消息
                        //Console.WriteLine($"[{host}:{port}] Received: {message}");

                    }
                    else
                    {
                        //Console.WriteLine($"[{host}:{port}] Server closed connection.");
                        //Logger.WriteLog($"[{host}:{port}] Server closed connection.");//该服务器关闭连接
                        Reconnect();  // 如果服务器关闭连接，则尝试重连
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
               // Logger.WriteLog($"[{host}:{port}] Receive error: {ex.Message}");
                //Console.WriteLine($"[{host}:{port}] Receive error: {ex.Message}");
                Reconnect();  // 如果接收出错，尝试重连
            }
        }


        // 重连机制
        private void Reconnect()
        {
            lock (socketLock)
            {
                if (socket != null)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both); // 关闭连接
                    }
                    catch { }
                    try
                    {       
                        socket.Close();
                    }
                    catch { }  // 关闭socket
                    socket = null;  // 清空socket
                }
            }
           // Logger.WriteLog($"[{host}:{port}] Reconnecting...");
            //Console.WriteLine($"[{host}:{port}] Reconnecting...");
            ConnectAsync().Wait();  // 等待重新连接
        }

        //将接收到的字符串消息添加到messageCache缓存栈中，限制最大缓存量
        private void AddToCache(string message)
        {
            if (messageCache.Count >= MaxCacheSize)
            {
                messageCache.TryDequeue(out _);  // 超过最大缓存时移除最旧的消息
            }
            messageCache.Enqueue(message);  // 将新的消息添加到队列中
        }

        //获取缓存中的所有消息（按先进先出）
        public IEnumerable<string> GetCachedMessages()
        {
            return messageCache.ToArray();
        }

        //// 启动心跳机制，定期发送心跳包
        //public void StartHeartbeat()
        //{
        //    Task.Run(new Action(() =>
        //    {
        //        while (isRunning && socket != null && socket.Connected)
        //        {
        //            try
        //            {
        //                // 创建并发送心跳消息
        //                string heartbeat = $"Heartbeat from port {port} at {DateTime.Now}";
        //                InternalSend(heartbeat);  // 调用内部的发送方法
        //                Console.WriteLine($"[Port {port}] Sent heartbeat");
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"[Port {port}] Heartbeat send error: {ex.Message}");
        //                Reconnect();  // 如果发送失败，尝试重连
        //            }

        //            Thread.Sleep(3000);  // 每3秒发送一次心跳
        //        }
        //    }
        //    ));
        //}

        //清除服务器最近接收到的一条消息
        public void StrClear()
        {
            lock (LastReceivedMessageLock)
            {
                LastReceivedMessage = null;
            }
        }

        // 向服务器发送消息
        public void Send(string message)
        {
            StrClear();
            if (socket == null || !socket.Connected)
            {
                Reconnect(); //断连时尝试重连
            }

            try
            {
                InternalSend(message);  // 调用内部的发送方法

            }
            catch (Exception ex)
            {
               Reconnect();
                  
            }
        }

        // 内部发送消息的实现
        private void InternalSend(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);  // 将消息转换为字节数组
            lock (socketLock)  // 锁定socket，防止并发发送
            {
                if (socket != null && socket.Connected)
                {
                    socket.Send(data);  // 发送消息

                }
                else
                {
                    throw new Exception("Socket not connected");  // 如果socket未连接，抛出异常
                }
            }
        }

        // 停止客户端连接
        public void Stop()
        {
            isRunning = false;  // 设置停止标志
            lock (socketLock)  // 锁定socket
            {
                if (socket != null)
                {
                    try
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                    try
                    {
                        socket.Close();
                    }
                    catch { }
                    isConnected = socket.Connected;
                    socket = null;  // 清空socket
                }
            }
        }
    }
}