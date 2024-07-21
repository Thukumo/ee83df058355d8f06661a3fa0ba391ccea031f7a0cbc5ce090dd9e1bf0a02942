using System.Net.Sockets;
using System.Text;

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
        public static async Task<string> IsPortOpenAsync(string ip, int port, int timeout)
        {
            try
            {
                Socket socket = new(SocketType.Stream, ProtocolType.Tcp){Blocking = false, SendTimeout = timeout, ReceiveTimeout = timeout}; //C#はint同士の除算のあまりを切り捨てるので放置
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                await socket.ConnectAsync(ip, port);
                byte[] msg = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\nHost: "+ip+"\r\nConnection: close\r\n\r\n");
                await socket.SendAsync(msg, SocketFlags.None);
                byte[] buffer = new byte[1024];
                await socket.ReceiveAsync(buffer, SocketFlags.None);
                //Console.WriteLine(Encoding.ASCII.GetString(buffer));
                socket.Close();
            }
            catch(Exception ex) when (ex is SocketException || ex is TaskCanceledException || ex is ObjectDisposedException)
            {
                return "";
            }
            return ip;
        }
        public static void Main(string[] args)
        {
            int port = 80;
            int timeout = 30;
            bool memo = false;
            for(int i = 0; i < args.Length; i++) //てきとーに書いた きたない
            {
                if(("TIMEOUT".Equals(args[i], StringComparison.OrdinalIgnoreCase) || "TO".Equals(args[i], StringComparison.OrdinalIgnoreCase)) && i + 1 < args.Length) memo = int.TryParse(args[i+1], out timeout);
                else
                {
                    if(memo) memo = false;
                    else _ = int.TryParse(args[i], out port);
                }
            }
            Console.Error.WriteLine("Port: " + port);
            Console.Error.WriteLine("Timeout: " + timeout);
            int[] arr = Enumerable.Range(1, 255).ToArray();
            Random random = new();
            //Parallel.ForEach(arr.OrderBy(x => new Random().Next()), new ParallelOptions(){MaxDegreeOfParallelism = 8}, (i) =>
            Parallel.ForEach(arr.OrderBy(x => new Random().Next()), (i) =>
            {
                List<Task<string>> tasks = [];
                foreach(int j in arr.OrderBy(x => random.Next())) foreach(int k in arr.OrderBy(x => random.Next()))
                {
                    for(int l = 0; l < 256; l++)if(IsValid(i, j)) tasks.Add(IsPortOpenAsync($"{i}.{j}.{k}.{l}", port, timeout));
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
