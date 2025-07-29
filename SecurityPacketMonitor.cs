// SecurityPacketMonitor.cs
using PacketDotNet;
using SharpPcap;
using System;
using System.IO;

namespace PacketMonitorWpf
{
    public class SecurityPacketMonitor
    {
        public event Action<string> OnAlert;

        private static readonly string[] BlacklistedIPs = { "192.168.8.155", "10.0.0.99" };
        private static readonly int[] DangerousPorts = { 21, 23, 445 }; // FTP, Telnet, SMB

        private ICaptureDevice _device;

        public void Start(ICaptureDevice device)
        {
            _device = device;
            _device.OnPacketArrival += OnPacketArrival;
            _device.Open();
            _device.StartCapture();
        }

        public void Stop()
        {
            _device?.StopCapture();
            _device?.Close();
        }

        private void OnPacketArrival(object sender, PacketCapture capture)
        {
            try
            {
                var packet = Packet.ParsePacket(capture.GetPacket().LinkLayerType, capture.GetPacket().Data);
                var ipPacket = packet.Extract<IPv4Packet>();
                if (ipPacket == null) return;

                string srcIp = ipPacket.SourceAddress.ToString();
                string dstIp = ipPacket.DestinationAddress.ToString();

                var tcp = packet.Extract<TcpPacket>();
                var udp = packet.Extract<UdpPacket>();

                int? port = tcp?.DestinationPort ?? udp?.DestinationPort;

                if (Array.Exists(BlacklistedIPs, ip => ip == srcIp || ip == dstIp))
                {
                    Alert($"🚨 블랙리스트 IP 접근 감지: {srcIp} → {dstIp}");
                }

                if (port != null && Array.Exists(DangerousPorts, p => p == port))
                {
                    Alert($"⚠️ 위험 포트 접근 감지 (Port {port}): {srcIp} → {dstIp}");
                }
            }
            catch (Exception ex)
            {
                Alert($"[오류] {ex.Message}");
            }
        }

        private void Alert(string message)
        {
            OnAlert?.Invoke(message);
            string logPath = @"C:\git\logs\security_log.txt";
            File.AppendAllText(logPath, $"{DateTime.Now}: {message}\n");
        }
    }
}
