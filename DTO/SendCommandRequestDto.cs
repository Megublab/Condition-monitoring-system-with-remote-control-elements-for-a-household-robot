namespace Registration_API.DTO
{
    public class SendCommandRequestDto
    {
        public Guid DeviceId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string CommandText { get; set; }
    }
}
