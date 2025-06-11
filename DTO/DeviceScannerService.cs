using System.Net.Sockets;

namespace Registration_API.DTO
{
    public class DeviceScannerService
    {
        public async Task<List<DeviceScanResult>> ScanLocalNetwork(string baseIp, int port = 12345)
        {
            var results = new List<DeviceScanResult>();
            var tasks = new List<Task>();

            for (int i = 1; i <= 254; i++)
            {
                string ip = $"{baseIp}.{i}";
                tasks.Add(Task.Run(async () =>
                {
                    bool isOnline = await IsDeviceOnline(ip, port);
                    lock (results)
                    {
                        results.Add(new DeviceScanResult
                        {
                            Ip = ip,
                            Port = port,
                            IsOnline = isOnline
                        });
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results.Where(r => r.IsOnline).ToList();
        }

        private async Task<bool> IsDeviceOnline(string ip, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ip, port);
                    var timeoutTask = Task.Delay(500); // 500 мс

                    var completed = await Task.WhenAny(connectTask, timeoutTask);
                    return completed == connectTask && client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }
    }

}
