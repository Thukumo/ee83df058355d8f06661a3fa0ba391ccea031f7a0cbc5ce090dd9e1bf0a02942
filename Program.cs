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
        public static async Task<string> IsPortOpenAsync(string ip, int port, int timeout, bool tcp, bool ignore_err)
        {
            Socket? socket = null;
            try
            {
                socket = new(SocketType.Stream, ProtocolType.Tcp){Blocking = false};
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                return await Task.WhenAny(Task.Run(async () => 
                {
                    try
                    {
                        var tes = Task.Delay(timeout*500+1000);
                        if(await Task.WhenAny(Task.Run(async () =>  {await socket.ConnectAsync(ip, port);}), tes) == tes) return false;
                        if(!tcp)
                        {
                            await socket.SendAsync(Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: " + ip + "\r\nConnection: close\r\n\r\n"), SocketFlags.None);
                            byte[] buffer = new byte[1024];
                            await socket.ReceiveAsync(buffer, SocketFlags.None);
                            //Console.WriteLine(ip+" "+Encoding.ASCII.GetString(buffer).Split(' ')[1]);
                            if(ignore_err) return !Encoding.ASCII.GetString(buffer).Split(' ')[1].StartsWith('4');
                        }
                        return true;
                    }
                    catch(Exception ex) when (ex is SocketException || ex is TaskCanceledException || ex is ObjectDisposedException)
                    {
                        return false;
                    }
                }), Task.Run(async () => 
                {
                    await Task.Delay(timeout*1000);
                    return false;
                })).Result? ip : "";
            }
            catch(Exception ex) when (ex is SocketException || ex is TaskCanceledException || ex is ObjectDisposedException)
            {
                return "";
            }
            finally
            {
                socket?.Close();
            }
            /*
            {
                try
                {
                    return (await new HttpClient{Timeout = TimeSpan.FromSeconds(timeout)}.GetAsync("http://" + ip + ":" + port + "/")).IsSuccessStatusCode? ip : "";
                }
                catch(Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    return "";
                }
            }
            */
        }
        public static void Main(string[] args)
        {
            int port = 80, timeout = 30;
            bool tcp = false, memo = false, ignore_err = false;
            for(int i = 0; i < args.Length; i++)
            {
                tcp |= "TCP".Equals(args[i], StringComparison.OrdinalIgnoreCase);
                ignore_err |= "IGNORE".Equals(args[i], StringComparison.OrdinalIgnoreCase) || "IG".Equals(args[i], StringComparison.OrdinalIgnoreCase);
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
            Console.Error.WriteLine("Ignore Error: " + ignore_err);
            int[] arr = Enumerable.Range(0, 256).ToArray();
            Random random = new();
            //Parallel.ForEach(arr.OrderBy(x => new Random().Next()), new ParallelOptions(){MaxDegreeOfParallelism = 8}, (i) => //CPU使用率がかなりマシになったので制限は不要
            Parallel.ForEach(arr.OrderBy(x => new Random().Next()), (i) =>
            {
                List<Task<string>> tasks = [];
                foreach(int j in arr.OrderBy(x => random.Next())) if(IsValidGlovalIP(i, j)) foreach(int k in arr.OrderBy(x => random.Next()))
                {
                    for(int l = 0; l < 256; l++) tasks.Add(IsPortOpenAsync($"{i}.{j}.{k}.{l}", port, timeout, tcp, ignore_err));
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
