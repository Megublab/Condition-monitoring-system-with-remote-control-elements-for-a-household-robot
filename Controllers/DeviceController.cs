using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Registration_API.Data;
using Registration_API.DTO;
using Registration_API.Models;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static Registration_API.Controllers.DeviceController;



namespace Registration_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        [HttpGet("ping")]
        public async Task<IActionResult> PingDevice([FromQuery] string ip, [FromQuery] int port)
        {
            try
            {
                using TcpClient client = new TcpClient();
                var connectTask = client.ConnectAsync(ip, port);
                var timeoutTask = Task.Delay(2000); 

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);

                if (completedTask == timeoutTask || !client.Connected)
                {
                    return BadRequest(new { success = false, message = "Пристрій недоступний або не відповідає." });
                }

                using NetworkStream stream = client.GetStream();


                var message = Encoding.UTF8.GetBytes("ping");
                await stream.WriteAsync(message, 0, message.Length);


                byte[] buffer = new byte[1024];
                var readTask = stream.ReadAsync(buffer, 0, buffer.Length);
                var readCompleted = await Task.WhenAny(readTask, Task.Delay(2000)); 

                if (readCompleted != readTask)
                {
                    return BadRequest(new { success = false, message = "Пристрій не відповідає (таймаут читання)." });
                }

                int bytesRead = readTask.Result;

                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (response.Contains("PONG") || response.Contains("HELLO_ROBOT"))
                {
                    return Ok(new { success = true, message = "Пристрій активний", response });
                }
                else
                {
                    return BadRequest(new { success = false, message = "Невірна відповідь пристрою." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Помилка підключення", error = ex.Message });
            }
        }

        private async Task<DeviceScanResult> CheckDeviceAsync(string ip, int port)
        {
            try
            {
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(2)
                };

                var url = $"http://{ip}:{port}/ping";

                HttpResponseMessage response = await httpClient.GetAsync(url);
                string responseBody = await response.Content.ReadAsStringAsync();

                return new DeviceScanResult
                {
                    Ip = ip,
                    Port = port,
                    IsOnline = response.IsSuccessStatusCode &&
                               (responseBody.Contains("PONG") || responseBody.Contains("HELLO_ROBOT")),
                    Response = responseBody,
                    Protocol = "HTTP"
                };
            }
            catch (Exception ex)
            {
                return new DeviceScanResult
                {
                    Ip = ip,
                    Port = port,
                    IsOnline = false,
                    Response = $"HTTP error: {ex.Message}",
                    Protocol = "HTTP"
                };
            }
        }


        [HttpPost("fetch-commands")]
        public async Task<IActionResult> FetchAndStoreDeviceCommands([FromQuery] string ip, [FromQuery] int port, [FromQuery] Guid deviceId)
        {
            string[] testCommands = new[] { "/commands", "/help", "/?", "/info", "/list" };

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

            foreach (var command in testCommands)
            {
                try
                {
                    var url = $"http://{ip}:{port}{command}";
                    var response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode) continue;

                    var content = await response.Content.ReadAsStringAsync();

                    var json = JsonDocument.Parse(content);
                    if (!json.RootElement.TryGetProperty("available_commands", out var commandsArray))
                        continue;

                    var commands = commandsArray.EnumerateArray().Select(x => x.GetString()).Where(x => !string.IsNullOrEmpty(x)).ToList();

                    if (!commands.Any())
                        continue;

                    var oldCommands = _context.device_commands.Where(c => c.device_id == deviceId);
                    _context.device_commands.RemoveRange(oldCommands);

                    foreach (var cmd in commands)
                    {
                        _context.device_commands.Add(new DeviceCommand
                        {
                            device_id = deviceId,
                            command_text = cmd!
                        });
                    }

                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        success = true,
                        message = "Команди отримано та збережено",
                        matchedCommand = command,
                        commands
                    });
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            return BadRequest(new { success = false, message = "Не вдалося знайти жодної команди у пристрою." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDevicesAsync()
        {
            var subnets = new List<string> { "192.168.0", "192.168.1", "192.168.100" };
            var commonPorts = new List<int> { 12345, 8888, 80, 443, 22, 8080 };

            var results = new List<DeviceSearchResult>();
            var semaphore = new SemaphoreSlim(50);

            var tasks = new List<Task>();

            foreach (var subnet in subnets)
            {
                for (int i = 1; i <= 254; i++)
                {
                    string ip = $"{subnet}.{i}";

                    foreach (var port in commonPorts)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync(); // БЕЗ токена
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
                            try
                            {
                                var result = await TryConnectAndIdentifyAsync(ip, port, cts.Token);
                                if (result != null)
                                {
                                    lock (results)
                                    {
                                        results.Add(result);
                                    }
                                }
                            }
                            catch
                            {

                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }));
                    }
                }
            }

            await Task.WhenAll(tasks);
            return Ok(results);
        }

        private async Task<DeviceSearchResult?> TryConnectAndIdentifyAsync(string ip, int port, CancellationToken cancellationToken)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(1.5)
                };

                var url = $"http://{ip}:{port}/name";

                var response = await client.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return null;

                var name = await response.Content.ReadAsStringAsync(cancellationToken);
                name = name.Trim();

                return new DeviceSearchResult
                {
                    IpAddress = ip,
                    Port = port,
                    DeviceName = string.IsNullOrWhiteSpace(name) ? $"{ip}:{port}" : name
                };
            }
            catch
            {
                return null;
            }
        }

        [HttpPost("delete/{userId}")]
        public async Task<IActionResult> DeleteUserDevice(int userId)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);

            if (user == null)
            {
                return NotFound($"Користувача з ID {userId} не знайдено.");
            }

            user.device_id = NoDeviceId;

            await _context.SaveChangesAsync();

            return Ok("Пристрій користувача успішно відв’язано.");
        }



        private readonly AppDbContext _context;
        private static readonly Guid NoDeviceId = Guid.Parse("00000000-0000-0000-0000-000000000000");

        public DeviceController(AppDbContext context)
        {
            _context = context;
        }

    }

}
