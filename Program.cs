using System.Net.Sockets;

namespace ip
{
    internal class Program
    {
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
            if(tcp)
            {
                using TcpClient tcpClient = new();
                try
                {
                    tcpClient.Connect(ip, port);
                    return tcpClient.Client.Connected? ip : "";
                }
                catch (SocketException)
                {
                    return "";
                }
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
            bool memo = false;
            for(int i = 0; i < args.Length; i++) //てきとーに書いた きたない
            {
                tcp |= "TCP".Equals(args[i], StringComparison.OrdinalIgnoreCase);
                if(("TIMEOUT".Equals(args[i], StringComparison.OrdinalIgnoreCase) || "TO".Equals(args[i], StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length)
                {
                    memo = int.TryParse(args[i+1], out timeout);
                }
                else
                {
                    if(memo) memo = false;
                    else _ = int.TryParse(args[i], out port);
                }
            }
            //List<HttpClient> httplis = []; //使いまわすHttpClientを入れとく?
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
                    for(int l = 0; l < 256; l++)if(IsValid(i, j)) tasks.Add(IsPortOpenAsync($"{i}.{j}.{k}.{l}", port, timeout, tcp));
                    Task.WhenAll(tasks).Wait();
                    foreach(Task<string> task in tasks) if(task.Result != "")
                    {
                        Console.WriteLine(task.Result);
                        Console.Error.WriteLine(task.Result);
                    }
                    tasks.Clear();
                }
            });
            //ないとは思うけど念のため
            Console.Error.WriteLine("探索が終了しました。");
        }
    }
}
