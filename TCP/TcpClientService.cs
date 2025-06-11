using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Registration_API.TCP
{
    public class TcpClientService
    {
        private string _ipAddress;
        private int _port;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private StreamWriter _writer;
        private StreamReader _reader;

        public TcpClientService(string ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
            _tcpClient = new TcpClient();
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                await _tcpClient.ConnectAsync(_ipAddress, _port);
                _networkStream = _tcpClient.GetStream();
                _writer = new StreamWriter(_networkStream, Encoding.ASCII);
                _reader = new StreamReader(_networkStream, Encoding.ASCII);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to device: {ex.Message}");
                return false;
            }
        }

        public async Task SendMessageAsync(string message)
        {
            try
            {
                if (_writer != null && _tcpClient.Connected)
                {
                    await _writer.WriteLineAsync(message);
                    await _writer.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public async Task<string> ReceiveMessageAsync()
        {
            try
            {
                if (_reader != null && _tcpClient.Connected)
                {
                    return await _reader.ReadLineAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving message: {ex.Message}");
                return null;
            }
        }

        public void CloseConnection()
        {
            _reader?.Dispose();
            _writer?.Dispose();
            _networkStream?.Dispose();
            _tcpClient?.Close();
        }

        public async Task StartDeviceAsync()
        {
            await SendMessageAsync("START");
        }

        public async Task StopDeviceAsync()
        {
            await SendMessageAsync("STOP");
        }


        public async Task PauseDeviceAsync()
        {
            await SendMessageAsync("PAUSE");
        }


        public async Task<string> GetStatusAsync()
        {
            await SendMessageAsync("STATUS");
            return await ReceiveMessageAsync();
        }

        public async Task ResetDeviceAsync()
        {
            await SendMessageAsync("RESET");
        }
    }
}
