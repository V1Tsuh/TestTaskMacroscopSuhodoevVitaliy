using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace TestTaskMacroscopSuhodoevVitaliy
{
    class Client
    {
        static string serverIp = "127.0.0.1"; // IP адрес сервера
        static int port = 1234; // Порт сервера

        static void Main(string[] args)
        {
            string folderPath = @"C:\work\TestTaskMacroscopSuhodoevVitaliy\requests";

            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Requests folder not found.");
                return;
            }

            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                SendRequest(filePath); // каждый файл как отдельный запрос
            }
        }

        static void SendRequest(string filePath)
        {
            using (TcpClient client = new TcpClient(serverIp, port))
            using (NetworkStream stream = client.GetStream())
            {
                string text = File.ReadAllText(filePath); // Читаем содержимое файла
                byte[] data = Encoding.UTF8.GetBytes(text); // Преобразуем текст в байты
                stream.Write(data, 0, data.Length); // Отправляем содержимое файла на сервер

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead); // Получаем ответ от сервера
                Console.WriteLine($"File: {Path.GetFileName(filePath)}, Response: {response}");
            }
        }
    }
}
