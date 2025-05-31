using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AkribisFAM.Util;

namespace AkribisFAM.CommunicationProtocol
{
    public class Task_CreateMesSocket
    {
        public static int CreateNewSocket()
        {

            TCPNetworkManage.clientNameToEndpoint.TryGetValue(ClientNames.mes, out var endpoint);
            TcpClient client = new TcpClient();
            TcpClient newClient = new TcpClient();
            int maxRetry = 3;

            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    newClient.Connect(endpoint.ip, endpoint.port);
                    lock (GlobalManager.Current.tcpQueue)
                    {
                        GlobalManager.Current.tcpQueue.Enqueue(newClient);
                    }
                }
                catch
                {
                    if (i < maxRetry)
                    {
                        Thread.Sleep(500);
                        Logger.WriteLog("try to reconnect MES server");
                    }
                    else
                    {
                        Logger.WriteLog("Connect MES server failed");
                        throw new Exception();
                    }

                    return 1;
                    
                }
            }

            return 1;

        }

        public static void UploadMessage() 
        {
            lock (GlobalManager.Current.tcpQueue)
            {
                TcpClient lastClient = GlobalManager.Current.tcpQueue.Dequeue();
                int res = Write(lastClient, "msg");
                lastClient.Close();
            }
            
        }

        public static string Compose(string input , string station_name)
        {

            string res = $"sfc_post @c = QUERY_4_SFC & subcmd = get_test_record & carrier_sn = {input} & station_code = BBE9 & station_id = {station_name} &";
            return res;
        }

        public static int Write(TcpClient client , string req)
        {
            try 
            {
                NetworkStream stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(req);

                stream.Write(data, 0, data.Length);

                stream.Flush(); 

                Logger.WriteLog("Barcode sent successfully.");

                return 0;
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error sending barcode: {ex.Message}");

                return 1;
            }
        }

        public static string Read(TcpClient client)
        {
            try
            {
                byte[] responseData = new byte[1024];
                NetworkStream stream = client.GetStream();
                int bytesRead = stream.Read(responseData, 0, responseData.Length);

                if (bytesRead > 0)
                {
                    string response = Encoding.UTF8.GetString(responseData, 0, bytesRead);
                    Logger.WriteLog($"Received response: {response}");
                    return response;  
                }

                return null;

            }
            catch (Exception ex)
            {
                Logger.WriteLog($"Error Read TcpClient Message: {ex.Message}");

                return null;
            }
        }
    }
}
