using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Registration_API.Models
{

    public class DeviceCommand
    {
        [Key]
        public int command_id { get; set; }
        public Guid device_id { get; set; }
        public string command_text { get; set; }
        public string description { get; set; }
        public DateTime added_at { get; set; }

        public Device Device { get; set; }
    }

}
