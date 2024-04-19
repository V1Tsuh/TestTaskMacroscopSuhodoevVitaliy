using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerApp
{
    class Server
    {
        static Dictionary<string, SemaphoreSlim> semaphores = new Dictionary<string, SemaphoreSlim>(); // Словарь с семафорами для каждого клиента
        static Dictionary<string, int> activeRequests = new Dictionary<string, int>(); // Словарь с количеством активных запросов для каждого клиента
        static bool ignoreCase = true;
        static int port = 1234;
        static int maxRequests = 1; // Максимальное количество одновременно обрабатываемых запросов для каждого клиента
        static readonly object lockObject = new object();

        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            Console.WriteLine($"Server started on port {port}");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClient));
                clientThread.Start(client);
            }
        }

        static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            string clientId = Guid.NewGuid().ToString(); // Генерируем уникальный идентификатор для каждого клиента

            // Создаем семафор для данного клиента, если его еще нет
            if (!semaphores.ContainsKey(clientId))
            {
                semaphores[clientId] = new SemaphoreSlim(maxRequests, maxRequests);
                lock (lockObject)
                {
                    activeRequests[clientId] = 0;
                }
            }

            // Проверяем доступность слота в семафоре для данного клиента
            if (!semaphores[clientId].Wait(0))
            {
                // Если нет доступных слотов, отправляем клиенту сообщение об ошибке и закрываем соединение
                SendErrorMessage(client);
                client.Close();
                return;
            }

            lock (lockObject)
            {
                activeRequests[clientId]++; // Увеличиваем количество активных запросов для данного клиента
            }


            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                Thread.Sleep(1000);

                string response = IsPalindrome(request) ? "YES" : "NO";
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
            finally
            {
                lock (lockObject)
                {
                    activeRequests[clientId]--; // Уменьшаем количество активных запросов для данного клиента
                }
                semaphores[clientId].Release(); // Освобождаем семафор для данного клиента
                client.Close();
            }
        }

        static void SendErrorMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            string errorMessage = "ERROR: Server overload. Please try again later.";
            byte[] errorBytes = Encoding.UTF8.GetBytes(errorMessage);
            stream.Write(errorBytes, 0, errorBytes.Length);
        }

        static bool IsPalindrome(string str)
        {
            if (ignoreCase)
                str = str.ToLower();

            int i = 0;
            int j = str.Length - 1;

            while (i < j)
            {
                if (str[i] != str[j])
                    return false;
                i++;
                j--;
            }

            return true;
        }
    }
}