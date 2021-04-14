using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace G.One_GUI
{
    using HidLibrary;
    public partial class Form1 : Form
    {
        private static HidDevice _device;
        private const int VID = 0xFEED;
        private const int PID = 0x6060;

        public Form1()
        {
            InitializeComponent();
        }
        public void AppendText(RichTextBox richTextBox, string text)
        {
            if (!richTextBox.InvokeRequired)
            {
                text += Environment.NewLine;
                richTextBox.AppendText(text);
            }
            else
            {
                richTextBox.Invoke(new Action<RichTextBox, string>(AppendText), richTextBox, text);
            }
        }
        private void Test_btn_Click(object sender, EventArgs e)
        {
            AppendText(richTextBox, "TEST 버튼 클릭");
        }
        public void Device()
        {
            _device = HidDevices.Enumerate(VID, PID).First();
            if (10 > _device.Capabilities.InputReportByteLength)
            {
                _device = HidDevices.Enumerate(VID, PID).Last();
            }

            if (_device != null)
            {
                _device.OpenDevice();

                _device.MonitorDeviceEvents = true;

                _device.Inserted += DeviceAttachedHandler;
                _device.Removed += DeviceRemovedHandler;

                string testString = "G.One 키보드 인식 완료";
                AppendText(richTextBox ,testString);
                Console.WriteLine("G.One 키보드 인식 완료");

                _device.ReadReport(OnReport);

                _device.CloseDevice();
            }
            else
            {
                MessageBox.Show("키보드 인식 실패!! 키보드 연결 후 확인을 눌러주세요.");
                Console.WriteLine("키보드 인식 실패");
                Device();
            }
            
        }
        private void OnReport(HidReport report)
        {
            var data = report.Data;
            var stringData = string.Empty;

            if (0 < data.Length)
            {
                stringData = Encoding.Default.GetString(data);
            }
            else
            {
                MessageBox.Show("에러!");
            }
            Console.WriteLine(stringData);
            Console.WriteLine(_device.Capabilities.InputReportByteLength);
            stringData += Environment.NewLine;
            AppendText(richTextBox, stringData);
            stringData = string.Empty;

            _device.ReadReport(OnReport);
        }

        private void DeviceAttachedHandler()
        {
            MessageBox.Show("기기가 연결 되었습니다.");
        }

        private void DeviceRemovedHandler()
        {
            MessageBox.Show("기기가 제거되었습니다.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Device();
        }
    }
}
