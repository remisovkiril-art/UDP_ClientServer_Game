using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
namespace UDP_Server;
internal class Program
{
    static readonly int MAX_ATTEMPTS = 10;
    static readonly int MIN_VALUE = 1;
    static readonly int MAX_VALUE = 100;
    static readonly int _PORT = 5001;
    static UdpClient _server = new UdpClient(_PORT);
    static Dictionary<string, ClientState> _clients = new Dictionary<string, ClientState>();
    static Random _random = new Random();
    static void Main(string[] args)
    {
        Console.WriteLine($"Server is listening on port {_PORT}...");

        while (true)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = _server.Receive(ref endPoint);

            string message = Encoding.UTF8.GetString(data).Trim();
            string key = endPoint.ToString();

            Console.WriteLine($"[{key}] -> {message}");

            MessageHandler(key, endPoint, message);
        }
    }
    static void MessageHandler(string key, IPEndPoint endPoint, string command)
    {
        if (command == "LIST")
        {
            string list = string.Join(", ", _clients.Values.Select(c => c.Name));
            Send(endPoint, $"PLAYERS: {list}");
            return;
        }
        if (command.StartsWith("JOIN"))
        {
            string name = command.Length > 5
                ? command.Substring(5)
                : "Unknown";
            if (_clients.ContainsKey(key))
            {
                Send(endPoint, "You already joined");
                return;
            }
            ClientState client = new ClientState
            {
                EndPoint = endPoint,
                Name = name,
                SecretNumber = _random.Next(MIN_VALUE, MAX_VALUE + 1),
                Attempts = 0
            };
            _clients[key] = client;

            Send(endPoint, $"Welcome {name}. Guess number 1-100");
            return;
        }
        if (!_clients.ContainsKey(key))
        {
            Send(endPoint, "ERROR: JOIN first");
            return;
        }
        var clientState = _clients[key];
        if (command == "EXIT" || command == "QUIT")
        {
            Console.WriteLine($"{clientState.Name} disconnected");
            _clients.Remove(key);

            Send(endPoint, "BYE");
            return;
        }
        if (command.StartsWith("GUESS"))
        {
            string numberPart = command.Substring(6);

            if (!int.TryParse(numberPart, out int guess))
            {
                Send(endPoint, "ERROR: enter number");
                return;
            }
            clientState.Attempts++;
            if (clientState.Attempts > MAX_ATTEMPTS)
            {
                Send(endPoint, "LOSE! Too many attempts. New game.");

                clientState.SecretNumber = _random.Next(MIN_VALUE, MAX_VALUE + 1);
                clientState.Attempts = 0;
                return;
            }
            if (guess < clientState.SecretNumber)
            {
                Send(endPoint, $"MORE (attempt {clientState.Attempts})");
            }
            else if (guess > clientState.SecretNumber)
            {
                Send(endPoint, $"LESS (attempt {clientState.Attempts})");
            }
            else
            {
                Send(endPoint, $"WIN! Attempts: {clientState.Attempts}");

                clientState.SecretNumber = _random.Next(MIN_VALUE, MAX_VALUE + 1);
                clientState.Attempts = 0;
            }
            return;
        }
        Send(endPoint, "ERROR: unknown command");
    }
    static void Send(IPEndPoint ep, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        _server.Send(data, data.Length, ep);
    }
}