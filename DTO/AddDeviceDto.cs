namespace Registration_API.DTO
{
    public class AddDeviceDto
    {
        public string DeviceName { get; set; }
        public int ModelId { get; set; }
        public int FirmwareId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
    }
}
