using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
namespace UDP_Server;
internal class ClientState
{
    public IPEndPoint? EndPoint { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SecretNumber { get; set; }
    public int Attempts { get; set; }
}