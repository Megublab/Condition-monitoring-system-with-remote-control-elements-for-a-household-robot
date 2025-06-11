using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Registration_API.Data;
using Registration_API.DTO;
using System.Net.Sockets;

namespace Registration_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class ConnectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConnectionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("connect/{userId}")]
        public async Task<IActionResult> ConnectToDevice(int userId, [FromBody] ConnectDeviceDto dto)
        {
            var user = await _context.users
                .Include(u => u.Device)
                .FirstOrDefaultAsync(u => u.user_id == userId);

            if (user == null || user.device_id == Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                return BadRequest(new { message = "User has no valid device." });
            }

            var device = user.Device;
            if (device == null)
            {
                return NotFound(new { message = "Device not found." });
            }

            // Тестове підключення по TCP
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(dto.DeviceIp, dto.Port);
                    if (await Task.WhenAny(connectTask, Task.Delay(3000)) != connectTask)
                    {
                        return StatusCode(504, new { message = "Connection timed out." });
                    }

                    if (!client.Connected)
                    {
                        return StatusCode(500, new { message = "Could not connect to the device." });
                    }
                }

                device.ip_address = dto.DeviceIp;
                device.port = dto.Port;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Connected and saved device network info." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error connecting to device", error = ex.Message });
            }
        }
    }

}
