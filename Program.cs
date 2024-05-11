using System.Net.Sockets;

namespace ip
{
    internal class Program
    {
        /*
        public static bool IsValidGlobalIP(string ip)
        {
            string[] splitValues = ip.Split('.');
            int a = int.Parse(splitValues[0]);
            int b = int.Parse(splitValues[1]);
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
        */
        public static bool IsValid(int a, int b)
        {
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
        public static async Task<string> IsPortOpenAsync(string ip, int port, int timeout, bool tcp = false)
        {
            {
                string[] splitValues = ip.Split('.');
                if(!IsValid(int.Parse(splitValues[0]), int.Parse(splitValues[1]))) return "";
            }
            if(tcp)
            {
                using TcpClient tcpClient = new();
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
                    HttpResponseMessage client = await new HttpClient{Timeout = TimeSpan.FromSeconds(timeout)}.GetAsync("http://" + ip + ":" + port + "/");
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
            int timeout = 30;
            bool tcp = false;
            /*
            foreach(string arg in args)
            {
                tcp |= "TCP".Equals(arg, StringComparison.OrdinalIgnoreCase);
                _ = int.TryParse(arg, out port);
            }
            */
            bool memo = false;
            for(int i = 0; i+1 < args.Length; i++) //てきとーに書いた きたない
            {
                if("TCP".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                {
                    tcp = true;
                }
                else if("TIMEOUT".Equals(args[i], StringComparison.OrdinalIgnoreCase) || "TO".Equals(args[i], StringComparison.OrdinalIgnoreCase))
                {
                    _ = int.TryParse(args[i+1], out timeout);
                    memo = true;
                }
                else if(!memo)
                {
                    _ = int.TryParse(args[i], out port);
                }
                else
                {
                    memo = false;
                }
            }
            Console.Error.WriteLine("Port: " + port);
            Console.Error.WriteLine("Timeout: " + timeout);
            Console.Error.WriteLine("Protocol: " + (tcp? "TCP" : "HTTP"));
            int[] arr = Enumerable.Range(1, 255).ToArray();
            Parallel.ForEach(arr.OrderBy(x => new Random().Next()), new ParallelOptions(){MaxDegreeOfParallelism = 8}, (i) =>
            {
                Random random = new();
                List<Task<string>> tasks = [];
                foreach(int j in arr.OrderBy(x => random.Next())) foreach(int k in arr.OrderBy(x => random.Next()))
                {
                    for(int l = 0; l < 256; l++) tasks.Add(IsPortOpenAsync($"{i}.{j}.{k}.{l}", port, timeout, tcp));
                    Task.WhenAll(tasks).Wait();
                    foreach(Task<string> task in tasks) if(task.Result != "")
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
