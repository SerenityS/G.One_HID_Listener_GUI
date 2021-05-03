using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HidLibrary;
using uPLibrary.Networking.M2Mqtt;

namespace G.One_GUI
{
    public partial class Form1 : Form
    {
        private List<HidDevice> _devices = new List<HidDevice>();

        public const ushort ConsoleUsagePage = 0xFF31;
        public const int ConsoleUsage = 0x0074;

        private readonly string strConn = "Server=gvsolgryn.nemiku.cc;Database=TestIoT;Uid=userID;Pwd=userPW;";
        
        public Form1()
        {
            InitializeComponent();
        }
        static class Underbar
        {
            public const string underbar = "-------------------------------------------------";
        }
        public void AppendText(RichTextBox richTextBox, string text)
        {
            if (!richTextBox.InvokeRequired)
            {
                text += Environment.NewLine;
                richTextBox.AppendText(text);
                richTextBox.ScrollToCaret();
            }
            else
            {
                richTextBox.Invoke(new Action<RichTextBox, string>(AppendText), richTextBox, text);
            }
        }
        public void Device(bool disconnected)
        {
            var devices = GetListableDevices().ToList();

            if (!disconnected)
            {
                foreach (var device in devices)
                {
                    var deviceExists = _devices.Aggregate(false, (current, dev) => current | dev.DevicePath.Equals(device.DevicePath));

                    if (device == null || deviceExists) continue;

                    _devices.Add(device);
                    device.OpenDevice();

                    device.Inserted += DeviceAttachedHandler;
                    device.Removed += DeviceRemovedHandler;

                    device.MonitorDeviceEvents = true;

                    AppendText(richTextBox, "G.One 키보드 장치 인식 완료");
                    device.ReadReport(OnReport);
                    device.CloseDevice();
                }
            }
        }
        private void OnReport(HidReport report)
        {
            var data = report.Data;
            var stringData = string.Empty;

            if (0 < data.Length)
            {
                stringData = Encoding.UTF8.GetString(data).Trim('\0');
            }
            else
            {
                MessageBox.Show("에러!");
            }
            Console.WriteLine(stringData);
            AppendText(richTextBox, stringData);
            HID_Status_Change(stringData);
            stringData = string.Empty;

            foreach (var device in _devices)
            {
                device.ReadReport(OnReport);
            }
        }
        private void DeviceAttachedHandler()
        {
            MessageBox.Show("기기가 연결 되었습니다");
        }
        private void DeviceRemovedHandler()
        {
            MessageBox.Show("기기가 제거 되었습니다");
        }
        private static IEnumerable<HidDevice> GetListableDevices() =>
            HidDevices.Enumerate()
                .Where(d => d.IsConnected)
                .Where(device => device.Capabilities.InputReportByteLength > 0)
                .Where(device => (ushort)device.Capabilities.UsagePage == ConsoleUsagePage)
                .Where(device => (ushort)device.Capabilities.Usage == ConsoleUsage);
        private void HID_Status_Change(string HID_Data)
        {
            if (HID_Data == "LED")
            {
                MySqlConnection conn = new MySqlConnection(strConn);
                conn.Open();
                string sql = "SELECT STATUS,SENSOR from test where ID='1'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader tableData = cmd.ExecuteReader();
                tableData.Read();
                Console.WriteLine("Status: {0}", tableData["STATUS"]);
                string name, topic;
                int status;
                status = (int)tableData["STATUS"];
                name = tableData["SENSOR"].ToString();
                topic = "LEDTopic";
                ChangeStatus(status, name, topic);
                tableData.Close();
                conn.Close();
            }
            else if (HID_Data == "MULTI")
            {
                MySqlConnection conn = new MySqlConnection(strConn);
                conn.Open();
                string sql = "SELECT STATUS,SENSOR from test where ID='2'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader tableData = cmd.ExecuteReader();
                tableData.Read();
                Console.WriteLine("Status: {0}", tableData["STATUS"]);
                string name, topic;
                int status;
                status = (int)tableData["STATUS"];
                name = tableData["SENSOR"].ToString();
                topic = "MULTITopic";
                ChangeStatus(status, name, topic);
                tableData.Close();
                conn.Close();
            }
        }
        public void TableLoad(string sql)
        {
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader tableData = cmd.ExecuteReader();

            while (tableData.Read())
            {
                Console.WriteLine("ID : {0}\n센서 : {1}\n상태 : {2}\n마지막 사용 시간 : {3}\n" + Underbar.underbar, tableData["ID"], tableData["SENSOR"], tableData["STATUS"], tableData["LAST_Use"]);

                string id, sensor, status, last;

                id = tableData["ID"].ToString();
                sensor = tableData["SENSOR"].ToString();
                status = tableData["STATUS"].ToString();
                last = tableData["LAST_Use"].ToString();

                AppendText(richTextBox, "ID : "+id+'\n'+"센서 이름 : "+sensor+'\n'+"상태 : "+status+'\n' + "마지막 사용 시간 : " + last + '\n' + Underbar.underbar + '\n');
            }
            tableData.Close();
            conn.Close();
        }

        public void ChangeStatus(int status, string name, string topic)
        {
            MqttClient client = new MqttClient("gvsolgryn.ddns.net");
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            string sql = "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            if (status == 0)
            {
                cmd.CommandText = "UPDATE test SET STATUS = 1, LAST_Use = now() WHERE SENSOR = @sensorName";
                cmd.Parameters.Add("@sensorName", MySqlDbType.VarChar, 20);
                cmd.Parameters[0].Value = name;
                cmd.ExecuteNonQuery();
                AppendText(richTextBox, name + " 켜기 완료!");
            }
            else if (status == 1)
            {
                cmd.CommandText = "UPDATE test SET STATUS = 0, LAST_Use = now() WHERE SENSOR = @sensorName";
                cmd.Parameters.Add("@sensorName", MySqlDbType.VarChar, 20);
                cmd.Parameters[0].Value = name;
                cmd.ExecuteNonQuery();
                AppendText(richTextBox, name + " 끄기 완료!");
            }
            else AppendText(richTextBox, "쿼리 에러");
            conn.Close();
            client.Connect("G_ONE_GUI", "userID", "userPW");

            if (status == 0)
            {
                if (topic == "LEDTopic")
                {
                    client.Publish(topic, Encoding.UTF8.GetBytes("1"));
                }
                else if (topic == "MULTITopic")
                {
                    client.Publish(topic, Encoding.UTF8.GetBytes("1"));
                }
            }
            else if (status == 1)
            {
                if (topic == "LEDTopic")
                {
                    client.Publish(topic, Encoding.UTF8.GetBytes("0"));
                }
                else if (topic == "MULTITopic")
                {
                    client.Publish(topic, Encoding.UTF8.GetBytes("0"));
                }
            }
        }
        private void DB_btn_Click(object sender, EventArgs e)
        {
            string sqlText = "SELECT * FROM test";
            TableLoad(sqlText);
        }
        private void LED_ON_OFF_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            string sql = "SELECT STATUS,SENSOR from test where ID='1'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader tableData = cmd.ExecuteReader();
            tableData.Read();
            string name, topic;
            int status;
            status = (int)tableData["STATUS"];
            name = tableData["SENSOR"].ToString();
            topic = "LEDTopic";
            ChangeStatus(status, name, topic);
            tableData.Close();
            conn.Close();
        }

        private void MULTITAP_ON_OFF_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            string sql = "SELECT STATUS,SENSOR from test where ID='2'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader tableData = cmd.ExecuteReader();
            tableData.Read();
            string name, topic;
            int status;
            status = (int)tableData["STATUS"];
            name = tableData["SENSOR"].ToString();
            topic = "MULTITopic";
            ChangeStatus(status, name, topic);
            tableData.Close();
            conn.Close();
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            Device(false);
        }
        private void Form1_FormClosing(object sender, FormClosedEventArgs e)
        {
            
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {

        }

        private void ToolStrip_Open_Click(object sender, EventArgs e)
        {

        }

        private void ToolStrip_Close_Click(object sender, EventArgs e)
        {

        }
    }
}
