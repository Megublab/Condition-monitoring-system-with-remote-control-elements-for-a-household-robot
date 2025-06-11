using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Registration_API.Data;
using Registration_API.DTO;
using Registration_API.Models;
using System;
using System.Threading.Tasks;

namespace Registration_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("userinfo")]
        public async Task<IActionResult> GetUserInfo([FromBody] UserInfoRequestDto dto)
        {
            var user = await _context.users
                .Include(u => u.Device)
                    .ThenInclude(d => d.Model)
                .FirstOrDefaultAsync(u => u.user_id == dto.UserId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var device = user.Device;

            return Ok(new
            {
                user_id = user.user_id,
                email = user.email,
                password_hash = user.password_hash,
                device_id = device?.device_id,
                ip_address = device?.ip_address,
                port = device?.port,
                device_name = device?.device_name,
                model_name = device?.Model?.model_name
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.users
                .Include(u => u.Device)
                .FirstOrDefaultAsync(u => u.email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.password_hash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            var nullDeviceId = Guid.Parse("00000000-0000-0000-0000-000000000000");
            var deviceId = user.device_id;
            bool hasDevice = deviceId != nullDeviceId;

            return Ok(new
            {
                user_id = user.user_id,
                device_id = deviceId,
                has_device = hasDevice,
                user_name = user.username
            });
        }

        [HttpPut("add-device/{userId}")]
        public async Task<IActionResult> AddDevice(int userId, [FromBody] SimpleAddDeviceDto dto)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.user_id == userId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            Guid newDeviceId;
            do
            {
                newDeviceId = Guid.NewGuid();
            } while (await _context.devices.AnyAsync(d => d.device_id == newDeviceId));

            string modelName = await GetDeviceModelName(dto.IpAddress, dto.Port) ?? "Unknown Model";


            var existingModel = await _context.device_models.FirstOrDefaultAsync(m => m.model_name == modelName);
            if (existingModel == null)
            {
                existingModel = new DeviceModel
                {
                    model_name = modelName,
                    manufacturer = "AutoDetected Inc.",
                    description = "Automatically added"
                };
                _context.device_models.Add(existingModel);
                await _context.SaveChangesAsync(); 
            }

            //  прошивка
            string firmwareVersion = await GetDeviceFirmwareVersion(dto.IpAddress, dto.Port) ?? "1.0.0";
            var existingFirmware = await _context.firmware_versions.FirstOrDefaultAsync(f => f.version == firmwareVersion);
            if (existingFirmware == null)
            {
                existingFirmware = new FirmwareVersion
                {
                    version = firmwareVersion,
                    release_date = DateTime.UtcNow,
                    changelog = "Automatically detected"
                };
                _context.firmware_versions.Add(existingFirmware);
                await _context.SaveChangesAsync(); 
            }

            var newDevice = new Device
            {
                device_id = newDeviceId,
                device_name = dto.DeviceName,
                model_id = existingModel.model_id,
                firmware_id = existingFirmware.firmware_id,
                added_at = DateTime.UtcNow,
                ip_address = dto.IpAddress,
                port = dto.Port
            };

            _context.devices.Add(newDevice);

            user.device_id = newDevice.device_id;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Device added and linked to user.",
                device_id = newDevice.device_id
            });
        }
        private async Task<string?> GetDeviceModelName(string ip, int port)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var response = await client.GetAsync($"http://{ip}:{port}/get_model");
                if (response.IsSuccessStatusCode)
                {
                    var name = await response.Content.ReadAsStringAsync();
                    return name.Trim();
                }
            }
            catch { }
            return null;
        }

        private async Task<string?> GetDeviceFirmwareVersion(string ip, int port)
        {
            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                var response = await client.GetAsync($"http://{ip}:{port}/get_firmware");
                if (response.IsSuccessStatusCode)
                {
                    var version = await response.Content.ReadAsStringAsync();
                    return version.Trim();
                }
            }
            catch { }
            return null;
        }




    }
}
