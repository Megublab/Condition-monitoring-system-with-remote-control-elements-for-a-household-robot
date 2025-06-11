using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Registration_API.Data;
using Registration_API.DTO;
using Registration_API.Models;
using System.Net.Sockets;
using System.Text;

namespace Registration_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceCommandController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DeviceCommandController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("commands")]
        public async Task<IActionResult> GetDeviceCommands([FromBody] DeviceCommandRequestDto dto)
        {
            if (dto.DeviceId == Guid.Empty)
            {
                return BadRequest(new { message = "Device ID is required." });
            }

            var commands = await _context.device_commands
                .Where(c => c.device_id == dto.DeviceId)
                .Select(c => new
                {
                    c.command_id,
                    c.command_text
                })
                .ToListAsync();

            if (!commands.Any())
            {
                return NotFound(new { message = "No commands found for this device." });
            }

            return Ok(commands);
        }

        [HttpPost("send-command")]
        public async Task<IActionResult> SendHttpCommand([FromBody] SendCommandRequestDto dto)
        {
            if (dto.DeviceId == Guid.Empty || string.IsNullOrWhiteSpace(dto.IpAddress) || dto.Port <= 0 || string.IsNullOrWhiteSpace(dto.CommandText))
            {
                return BadRequest(new { message = "Invalid input data." });
            }

            try
            {
                using var httpClient = new HttpClient();
                var url = $"http://{dto.IpAddress}:{dto.Port}/{dto.CommandText.ToLower()}";

                HttpResponseMessage response;


                string[] getCommands = { "status", "commands", "name" };
                if (getCommands.Contains(dto.CommandText.ToLower()))
                {
                    response = await httpClient.GetAsync(url);
                }
                else
                {
                    var content = new StringContent(""); // Порожнє тіло
                    response = await httpClient.PostAsync(url, content);
                }

                var responseText = await response.Content.ReadAsStringAsync();

                var command = new DeviceCommand
                {
                    device_id = dto.DeviceId,
                    command_text = dto.CommandText,
                    description = "Sent via HTTP",
                    added_at = DateTime.UtcNow
                };

                _context.device_commands.Add(command);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Command '{dto.CommandText}' sent via {(getCommands.Contains(dto.CommandText.ToLower()) ? "GET" : "POST")}.",
                    device_response = responseText
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error sending HTTP command.", error = ex.Message });
            }
        }





    }

}
