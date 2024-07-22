using System.Net.Sockets;
using System.Text;

namespace ip
{
    internal class Program
    {
        public static bool IsValidGlovalIP(int a, int b)
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
        public static async Task<string> IsPortOpenAsync(string ip, int port, int timeout, bool tcp)
        {
            if(tcp)try
            {
                using Socket socket = new(SocketType.Stream, ProtocolType.Tcp) { Blocking = false, SendTimeout = timeout, ReceiveTimeout = timeout };
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                bool failed = false;
                var timeoutTask = Task.Run(async () => 
                {
                    await Task.Delay(timeout);
                    return "";
                });
                var completedTask = await Task.WhenAny(Task.Run(async () => 
                {
                    var tes = Task.Delay(10);
                    if(await Task.WhenAny(tes, Task.Run(async () => 
                    {
                        await socket.ConnectAsync(ip, port);
                    })) == tes)
                    {
                        failed = true;
                        return;
                    }
                    if(!tcp)
                    {
                        byte[] msg = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: " + ip + "\r\nConnection: close\r\n\r\n");
                        await socket.SendAsync(msg, SocketFlags.None);
                        byte[] buffer = new byte[1024];
                        await socket.ReceiveAsync(buffer, SocketFlags.None);
                        failed = !Encoding.ASCII.GetString(buffer).Split(' ')[1].StartsWith('2');
                    }
                }), timeoutTask);
                if (completedTask == timeoutTask || failed) return "";
                Console.Error.WriteLine(failed.ToString());
                socket.Close();
                return ip;
            }
            catch(Exception ex) when (ex is SocketException || ex is TaskCanceledException || ex is ObjectDisposedException)
            {
                return "";
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
                if (("TIMEOUT".Equals(args[i], StringComparison.OrdinalIgnoreCase) || "TO".Equals(args[i], StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length) if(memo = int.TryParse(args[i+1], out int tmp)) timeout = tmp;
                else
                {
                    if(memo) memo = false;
                    else if(int.TryParse(args[i], out tmp)) port = tmp;
                }
            }
            Console.Error.WriteLine("Port: " + port);
            Console.Error.WriteLine("Timeout: " + timeout);
            Console.Error.WriteLine("Protocol: " + (tcp? "TCP": "HTTP"));
            int[] arr = Enumerable.Range(0, 255).ToArray();
            Random random = new();
            Parallel.ForEach(arr.OrderBy(x => new Random().Next()), new ParallelOptions(){MaxDegreeOfParallelism = 8}, (i) =>
            //Parallel.ForEach(arr.OrderBy(x => new Random().Next()), (i) =>
            {
                List<Task<string>> tasks = [];
                foreach(int j in arr.OrderBy(x => random.Next())) foreach(int k in arr.OrderBy(x => random.Next()))
                {
                    for(int l = 0; l < 256; l++) if(IsValidGlovalIP(i, j)) tasks.Add(IsPortOpenAsync($"{i}.{j}.{k}.{l}", port, timeout, tcp));
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
