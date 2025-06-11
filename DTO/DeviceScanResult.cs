namespace Registration_API.DTO
{
    public class DeviceScanResult
    {

            public string Ip { get; set; }
            public int Port { get; set; }
            public bool IsOnline { get; set; }
            public string? Response { get; set; }
            public string? Protocol { get; set; } // TCP або HTTP

    }
}
