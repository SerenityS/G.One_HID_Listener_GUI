using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace G.One_GUI
{
    using HidLibrary;
    public partial class Form1 : Form
    {
        private static HidDevice _device;
        private const int VID = 0xFEED;
        private const int PID = 0x6060;
        private string strConn = "Server=gvsolgryn.nemiku.cc;Database=TestIoT;Uid=iot;Pwd=1q2w3e4r;";
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
        }
        private void DeviceRemovedHandler()
        {
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

                AppendText(richTextBox, "ID : "+id+'\n'+"센서 이름 : "+sensor+'\n'+"상태 : "+status+'\n' + "마지막 사용 시간" + last + '\n' + Underbar.underbar + '\n');
            }
            tableData.Close();
            conn.Close();
        }

        public void ChangeStatus(int status, string name)
        {
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
            string sql = "SELECT STATUS from test where SENSOR='led'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);

            if (status == 0)
            {
                //string test = "UPDATE test SET STATUS = 1, LAST_Use = now() WHERE SENSOR = @sensorName";
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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Device();
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
            Console.WriteLine("Status: {0}", tableData["STATUS"]);
            string name;
            int status;
            status = (int)tableData["STATUS"];
            name = tableData["SENSOR"].ToString();
            ChangeStatus(status, name);
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
            Console.WriteLine("Status: {0}", tableData["STATUS"]);
            string name;
            int status;
            status = (int)tableData["STATUS"];
            name = tableData["SENSOR"].ToString();
            ChangeStatus(status, name);
            tableData.Close();
            conn.Close();
        }
    }
}
