using System.Net;
using System.Net.Sockets;
using System.Text;
namespace UDP_Client
{
    internal class Program
    {
        static readonly int _PORT = 5001;
        static UdpClient _client = new UdpClient();
        static IPEndPoint _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), _PORT);
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your name:");
            string? name = Console.ReadLine();
            if (string.IsNullOrEmpty(name))
                return;
            Send($"JOIN {name}");
            Thread receiveThread = new Thread(Receive);
            receiveThread.IsBackground = true;
            receiveThread.Start();
            while (true)
            {
                string? input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                    continue;
                if (input.ToLower() == "exit" || input.ToLower() == "quit")
                {
                    Send("EXIT");
                    break;
                }
                if (input.ToLower() == "list")
                {
                    Send("LIST");
                    continue;
                }
                if (int.TryParse(input, out _))
                {
                    Send($"GUESS {input}");
                }
                else
                {
                    Console.WriteLine("Enter number, 'list' or 'exit'");
                }
            }
        }
        static void Send(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _client.Send(data, data.Length, _endPoint);
        }
        static void Receive()
        {
            while (true)
            {
                try
                {
                    byte[] data = _client.Receive(ref _endPoint);
                    string response = Encoding.UTF8.GetString(data);
                    Console.WriteLine($"Server: {response}");
                }
                catch
                {
                    break;
                }
            }
        }
    }
}