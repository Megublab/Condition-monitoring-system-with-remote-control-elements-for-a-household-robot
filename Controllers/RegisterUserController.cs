using Microsoft.AspNetCore.Mvc;
using Registration_API.Data;
using Registration_API.Models;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Registration_API.DTO;
using System.Text.RegularExpressions;
using BCrypt.Net;


namespace Registration_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Guid defaultDeviceId = Guid.Parse("00000000-0000-0000-0000-000000000000");

        public RegisterController(AppDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Username, email, and password are required." });
            }

            var emailRegex = new Regex(@"^[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            if (!emailRegex.IsMatch(dto.Email))
            {
                return BadRequest(new { message = "Invalid email format." });
            }

            if (await _context.users.AnyAsync(u => u.email == dto.Email))
            {
                return BadRequest(new { message = "Email already registered." });
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                username = dto.Username,
                email = dto.Email,
                password_hash = hashedPassword,
                device_id = defaultDeviceId,
                created_at = DateTime.UtcNow
            };

            _context.users.Add(user);
            await _context.SaveChangesAsync();

            var fullUser = await _context.users
                .Include(u => u.Device)
                    .ThenInclude(d => d.Model)
                .Include(u => u.Device)
                    .ThenInclude(d => d.Firmware)
                .FirstOrDefaultAsync(u => u.user_id == user.user_id);

            return Ok(new
            {
                fullUser.user_id,
                fullUser.username,
                fullUser.email,
                fullUser.created_at,

                Device = fullUser.Device == null ? null : new
                {
                    fullUser.Device.device_id,
                    fullUser.Device.device_name,
                    fullUser.Device.added_at,
                    fullUser.Device.ip_address,
                    fullUser.Device.port,

                    Model = fullUser.Device.Model == null ? null : new
                    {
                        fullUser.Device.Model.model_id,
                        fullUser.Device.Model.model_name,
                        fullUser.Device.Model.manufacturer,
                        fullUser.Device.Model.description
                    },

                    Firmware = fullUser.Device.Firmware == null ? null : new
                    {
                        fullUser.Device.Firmware.firmware_id,
                        fullUser.Device.Firmware.version,
                        fullUser.Device.Firmware.release_date,
                        fullUser.Device.Firmware.changelog
                    }
                }
            });
        }
    }
}
