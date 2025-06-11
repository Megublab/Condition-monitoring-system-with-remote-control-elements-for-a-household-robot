using System.ComponentModel.DataAnnotations;

namespace Registration_API.Models
{
    public class DeviceModel
    {
        [Key]
        public int model_id { get; set; }
        public string model_name { get; set; }
        public string? manufacturer { get; set; }
        public string? description { get; set; }

        public ICollection<Device> Devices { get; set; }
    }

}
