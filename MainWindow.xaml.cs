using System;
using System.Linq;
using System.Windows;
using SharpPcap;

namespace PacketMonitorWpf
{
    public partial class MainWindow : Window
    {
        private SecurityPacketMonitor monitor;

        public MainWindow()
        {
            InitializeComponent();
            LoadDevices();
        }

        private void LoadDevices()
        {
            try
            {
                var devices = CaptureDeviceList.Instance;

                if (devices.Count < 1)
                {
                    MessageBox.Show("캡처 가능한 네트워크 장치를 찾을 수 없습니다.");
                    return;
                }

                DeviceComboBox.ItemsSource = devices;
                DeviceComboBox.DisplayMemberPath = "Description";
                DeviceComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"장치 로드 오류: {ex.Message}");
            }
        }

        private void StartCapture_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceComboBox.SelectedItem is ICaptureDevice selectedDevice)
            {
            }
            else
            {
                MessageBox.Show("장치를 선택하세요.");
                return;
            }

            monitor = new SecurityPacketMonitor();
            monitor.OnAlert += msg =>
            {
                Dispatcher.Invoke(() => PacketLogList.Items.Add(msg));
            };

            monitor.Start(selectedDevice);
            PacketLogList.Items.Add("패킷 모니터링 시작됨: " + selectedDevice.Description);
        }

        private void StopCapture_Click(object sender, RoutedEventArgs e)
        {
            if(monitor != null)
            {
                monitor.Stop();
                PacketLogList.Items.Add("🛑 패킷 모니터링 종료됨");

                // 선택된 장치 이름도 표시 (선택되어 있다면)
                if (DeviceComboBox.SelectedItem is ICaptureDevice selectedDevice)
                {
                    PacketLogList.Items.Add("장치: " + selectedDevice.Description);
                }
            }
            else
            {
                PacketLogList.Items.Add("⚠️ 모니터가 실행 중이 아닙니다.");
            }
        }
    }
}
