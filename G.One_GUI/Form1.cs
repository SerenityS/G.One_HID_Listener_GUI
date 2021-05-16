using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using HidLibrary;
using uPLibrary.Networking.M2Mqtt;


namespace G.One_GUI
{
    public partial class Form1 : Form
    {
        private readonly List<HidDevice> _devices = new List<HidDevice>();
        public const ushort ConsoleUsagePage = 0xFF31;
        public const int ConsoleUsage = 0x0074;

        static readonly string Server = IDPW.DB_Server;
        static readonly string Database = IDPW.DB_DataBase;
        static readonly string Uid = IDPW.DB_UserID;
        static readonly string Pwd = IDPW.DB_UserPW;

        static readonly string MQTT_Host = IDPW.Mqtt_Server;
        static readonly string MQTT_ClientID = IDPW.Mqtt_ClientID;
        static readonly string MQTT_ID = IDPW.Mqtt_ID;
        static readonly string MQTT_PW = IDPW.Mqtt_PW;

        private readonly string strConn = string.Format(
            "Server={0};Database={1};Uid={2};Pwd={3};charset=utf8;",
            Server, Database, Uid, Pwd);
        private MySqlConnection Conn()
        {
            MySqlConnection conn = new MySqlConnection(strConn);

            return conn;
        }

        private readonly string mqttConn = MQTT_Host;



        public Form1()
        {
            InitializeComponent();

            Tray_Icon.MouseDoubleClick += Tray_Icon_MouseDoubleClick;
            showToolStripMenuItem.Click += ToolStrip_Open_Click;
            exitToolStripMenuItem.Click += ToolStrip_Close_Click;

            this.Load += Form1_Load;

            this.FormClosed += Form_Closing;
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
            MqttClient client = new MqttClient(mqttConn);
            conn.Open();
            string sql = "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            Stopwatch sw = new Stopwatch();

            if (status == 0)
            {
                cmd.ExecuteNonQuery();
                AppendText(richTextBox, name + " 켜기 완료!");
            }
                catch (Exception e)
                {
                    AppendText(richTextBox, "데이터 베이스 에러로그 : " + e.Message);
                    string use_Program = "G_ONE_GUI";
                    string seneor = name;
                    string def_location = name + "_ON";
                    string sql_success = "fail";
                    string error_log = e.Message;

                    cmd.CommandText = "INSERT INTO log(use_program, sensor, def_location, sql_success, error_log, sql_run_time) VALUES(@use_program, @sensor, @def_location, @sql_success, @error_log, @sql_run_time)";
                    cmd.Parameters.AddWithValue("@use_program", use_Program);
                    cmd.Parameters.AddWithValue("@sensor", seneor);
                    cmd.Parameters.AddWithValue("@def_location", def_location);
                    cmd.Parameters.AddWithValue("@sql_success", sql_success);
                    cmd.Parameters.AddWithValue("@error_log", error_log);
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    string elapsedTime = String.Format("{0:0}.{1:000}", ts.Seconds, ts.Milliseconds);
                    cmd.Parameters.AddWithValue("@sql_run_time", elapsedTime);
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    string use_Program = "G_ONE_GUI";
                    string seneor = name;
                    string def_location = name + "_ON";
                    string sql_success = "success";
                    string error_log = "none";

                    cmd.CommandText = "INSERT INTO log(use_program, sensor, def_location, sql_success, error_log, sql_run_time) VALUES(@use_program, @sensor, @def_location, @sql_success, @error_log, @sql_run_time)";
                    cmd.Parameters.AddWithValue("@use_program", use_Program);
                    cmd.Parameters.AddWithValue("@sensor", seneor);
                    cmd.Parameters.AddWithValue("@def_location", def_location);
                    cmd.Parameters.AddWithValue("@sql_success", sql_success);
                    cmd.Parameters.AddWithValue("@error_log", error_log);
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    string elapsedTime = String.Format("{0:0}.{1:000}", ts.Seconds, ts.Milliseconds);
                    cmd.Parameters.AddWithValue("@sql_run_time", elapsedTime);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            else if (status == 1)
            {
                cmd.ExecuteNonQuery();
                AppendText(richTextBox, name + " 끄기 완료!");
            }
                catch (Exception e)
                {
                    AppendText(richTextBox, "데이터 베이스 에러로그 : " + e.Message);
                    string use_Program = "G_ONE_GUI";
                    string seneor = name;
                    string def_location = name + "_OFF";
                    string sql_success = "fail";
                    string error_log = e.Message;

                    cmd.CommandText = "INSERT INTO log(use_program, sensor, def_location, sql_success, error_log, sql_run_time) VALUES(@use_program, @sensor, @def_location, @sql_success, @error_log, @sql_run_time)";
                    cmd.Parameters.AddWithValue("@use_program", use_Program);
                    cmd.Parameters.AddWithValue("@sensor", seneor);
                    cmd.Parameters.AddWithValue("@def_location", def_location);
                    cmd.Parameters.AddWithValue("@sql_success", sql_success);
                    cmd.Parameters.AddWithValue("@error_log", error_log);
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    string elapsedTime = String.Format("{0:0}.{1:000}", ts.Seconds, ts.Milliseconds);
                    cmd.Parameters.AddWithValue("@sql_run_time", elapsedTime);
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    string use_Program = "G_ONE_GUI";
                    string seneor = name;
                    string def_location = name + "_OFF";
                    string sql_success = "success";
                    string error_log = "none";

                    cmd.CommandText = "INSERT INTO log(use_program, sensor, def_location, sql_success, error_log, sql_run_time) VALUES(@use_program, @sensor, @def_location, @sql_success, @error_log, @sql_run_time)";
                    cmd.Parameters.AddWithValue("@use_program", use_Program);
                    cmd.Parameters.AddWithValue("@sensor", seneor);
                    cmd.Parameters.AddWithValue("@def_location", def_location);
                    cmd.Parameters.AddWithValue("@sql_success", sql_success);
                    cmd.Parameters.AddWithValue("@error_log", error_log);
                    sw.Stop();
                    TimeSpan ts = sw.Elapsed;
                    string elapsedTime = String.Format("{0:0}.{1:000}", ts.Seconds, ts.Milliseconds);
                    cmd.Parameters.AddWithValue("@sql_run_time", elapsedTime);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            else AppendText(richTextBox, "쿼리 에러");
            conn.Close();

            client.Connect(MQTT_ClientID, MQTT_ID, MQTT_PW);

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
            TableLoad(sqlText);
            Device(false);
        }
        private void LED_ON_OFF_Click(object sender, EventArgs e)
        {
            MySqlConnection conn = new MySqlConnection(strConn);
            conn.Open();
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
        private void Form1_Load(object sender, EventArgs e)
        {
            Device(false);
        }
        private void Tray_Icon_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowIcon = false; //작업표시줄에서 제거.
                Tray_Icon.Visible = true; //트레이 아이콘을 표시한다.
            }
        }
        public void TrayIcon_Load(object sender, FormClosedEventArgs e)
        {
            Tray_Icon.ContextMenuStrip = ContextMenuStrip;
        }
        private void Tray_Icon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }
        private void ToolStrip_Open_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }
        private void ToolStrip_Close_Click(object sender, EventArgs e)
        {
            MessageBox.Show("프로그램을 종료하시겠습니까?");
            this.Close();
        }
        public void Form_Closing(object sender, FormClosedEventArgs e)
        {
        }
    }
}
