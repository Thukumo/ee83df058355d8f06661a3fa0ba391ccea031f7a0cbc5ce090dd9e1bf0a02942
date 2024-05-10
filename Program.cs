using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace ip
{
    internal class Program
    {
    public static bool IsValidGlobalIP(string ip)
    {
        string[] splitValues = ip.Split('.');
        var a = int.Parse(splitValues[0]);
        var b = int.Parse(splitValues[1]);
        switch (a)
        {
            case 0:
            case 10:
            case 127:
            case 169 when b == 254:
            case 172 when 16 <= b && b <= 31:
            case 192 when b == 168:
                return false;
            default:
                return a < 224;
        }
    }
    public static async Task<string> IsPortOpenAsync(string ip, int port, bool tcp)
    {
        if(tcp)
        {
            using var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(ip, port);
            }
            catch (SocketException)
            {
                return "";
            }
            return tcpClient.Client.Connected? ip : "";
        }
        else
        {
            try
            {
                var client = await new HttpClient{Timeout = TimeSpan.FromSeconds(30)}.GetAsync("http://" + ip + ":" + port + "/");
                return client.IsSuccessStatusCode? ip : "";
            }
            catch(Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
            {
                return "";
            }
        }
    }
    public static void Main(string[] args)
    {
            int port = 80;
            bool tcp = false;
            foreach(var hoge in args)
            {
                if(hoge == "TCP") tcp = true;
                _ = int.TryParse(hoge, out port);
            }
            //var lis = new List<int>();
            //for(int i = 0; i < 256; i++) lis.Add(i);
            var lis = Enumerable.Range(1, 255).ToArray();
            Parallel.ForEach(lis.OrderBy(x => new Random().Next()), new ParallelOptions(){MaxDegreeOfParallelism = 8}, i =>
            {
                var random = new Random();
                string ip = "";
                var tasks = new List<Task<string>>();
                foreach(int j in lis.OrderBy(x => random.Next())) foreach(int k in lis.OrderBy(x => random.Next()))
                {
                    for(int l = 0; l < 256; l++) if(IsValidGlobalIP(ip = $"{i}.{j}.{k}.{l}")) tasks.Add(IsPortOpenAsync(ip, port, tcp));
                    Task.WhenAll(tasks).Wait();
                    foreach(var task in tasks) if(task.Result != "")
                    {
                        Console.WriteLine(task.Result);
                        Console.Error.WriteLine(task.Result);
                    }
                    tasks.Clear();
                }
            });
        }
    }
}
