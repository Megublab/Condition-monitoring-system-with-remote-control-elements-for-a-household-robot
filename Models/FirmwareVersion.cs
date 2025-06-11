using System.ComponentModel.DataAnnotations;

namespace Registration_API.Models
{


    public class FirmwareVersion
    {
        [Key]
        public int firmware_id { get; set; }
        public string version { get; set; }
        public DateTime? release_date { get; set; }
        public string? changelog { get; set; }

        public ICollection<Device> Devices { get; set; }
    }

}
