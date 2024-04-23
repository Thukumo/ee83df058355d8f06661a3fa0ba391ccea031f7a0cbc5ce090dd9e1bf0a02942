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
        /*
        if (string.IsNullOrWhiteSpace(ip))
        {
            return false;
        }
        if (splitValues.Length != 4)
        {
            return false;
        }
        if (!splitValues.All(r => byte.TryParse(r, out byte tempForParsing)))
        {
            return false;
        }
        */
        if (splitValues[0] == "0")
        {
            return false;
        }
        if (splitValues[0] == "10")
        {
            return false;
        }
        if (splitValues[0] == "127")
        {
            return false;
        }
        if (splitValues[0] == "169" && splitValues[1] == "254")
        {
            return false;
        }
        if (splitValues[0] == "172" &&  16 <= int.Parse(splitValues[1]) && int.Parse(splitValues[1]) <= 31)
        {
            return false;
        }
        if (splitValues[0] == "192" && splitValues[1] == "168")
        {
            return false;
        }
        if(224 <= int.Parse(splitValues[0])) //224.0.0.0~はマルチキャストアドレス、240.~は実験用アドレス、255.~はブロードキャストアドレス
        {
            return false;
        }
        return true;
    }
    public static async Task<bool> IsPortOpenAsync(string ip, int port, bool tcp)
    {
        if(tcp)
        {
            using var tcpClient = new System.Net.Sockets.TcpClient();
            try
            {
                tcpClient.Connect(ip, port);
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }
            return tcpClient.Client.Connected;
        }
        else
        {
            try
            {
                var hoge = await new HttpClient{Timeout = TimeSpan.FromSeconds(30)}.GetAsync("http://" + ip + ":" + port + "/");
                return hoge.IsSuccessStatusCode;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch(TaskCanceledException)
            {
                return false;
            }
        }
    }
public static async Task<string> RetipAsync(string ip, int port, bool tcp = false) 
{
  if(await Task.Run(() => IsPortOpenAsync(ip, port, tcp)))
  {
    return ip; 
  }
  return "";
}
    public static void Main(string[] args)
    {
        int port = 80;
        bool tcp = false;
        for(int i = 0; i < args.Length; i++)
        {
            var hoge = args[i];
            if(hoge == "TCP") tcp = true;
                _ = int.TryParse(args[i], out port);
        }
        Parallel.For(0, 256, i =>
        {
            string ip = "";
            var tasks = new List<Task<string>>();
            for(int j = 0; j < 256; j++)
            {
                for(int k = 0; k < 256; k++)
                {
                    for(int l = 0; l < 256; l++)
                    {
                        ip = $"{i}.{j}.{k}.{l}";
                        if(IsValidGlobalIP(ip)) tasks.Add(RetipAsync(ip, port, tcp));
                    }
                    Task.WhenAll(tasks).Wait();
                    foreach(var task in tasks) if(task.Result != "") Console.WriteLine(task.Result);
                    tasks.Clear();
                }
            }
        });
    }
    }
}
