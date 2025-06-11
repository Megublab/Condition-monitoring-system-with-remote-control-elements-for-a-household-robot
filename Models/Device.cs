using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Registration_API.Models
{
    public class Device
    {
        [Key]
        public Guid device_id { get; set; }

        public int model_id { get; set; }
        public int firmware_id { get; set; }
        public string? device_name { get; set; }
        public DateTime added_at { get; set; }
        public string ip_address { get; set; }
        public int port { get; set; }

        public DeviceModel Model { get; set; }
        public FirmwareVersion Firmware { get; set; }
        public ICollection<DeviceCommand> DeviceCommands { get; set; }
        public ICollection<User> Users { get; set; }
    }

}
