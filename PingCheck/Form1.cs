using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PingCheck
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<string> allResult = new List<string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbResult.Items.Add("----------");
            cmbResult.Items.Add("All");
            cmbResult.Items.Add("Success");
            cmbResult.Items.Add("Failed");
            cmbResult.SelectedIndex = 0;
        }

        private void WriteLog(string message,Color color)
        {
            allResult.Add(message);
            rtbResult.SelectionStart = rtbResult.TextLength;
            rtbResult.SelectionLength = 0;
            rtbResult.SelectionColor = color;
            rtbResult.AppendText(message + Environment.NewLine);

            rtbResult.SelectionColor = rtbResult.ForeColor;
        }

        private void FillList(string filter)
        {
            rtbResult.Clear();

            foreach (string item in allResult)
            {
                if (filter == "All" ||
                    (filter == "Success" && item.Contains("Success")) ||
                    (filter == "Failed" && item.Contains("Failed")))
                {
                    Color renk;

                    if (item.Contains("Ping: Failed"))
                        renk = Color.Red;
                    else
                        renk = Color.Green;

                    rtbResult.SelectionStart = rtbResult.TextLength;
                    rtbResult.SelectionLength = 0;
                    rtbResult.SelectionColor = renk;
                    rtbResult.AppendText(item + Environment.NewLine);
                    rtbResult.SelectionColor = rtbResult.ForeColor;
                }
            }
        }

        private async Task<bool> PingCheck(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 2000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> PortCheck(string ip, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Task connectTask = client.ConnectAsync(ip, port);
                    Task timeoutTask = Task.Delay(2000);

                    Task completed = await Task.WhenAny(connectTask, timeoutTask);

                    return completed == connectTask && client.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        private void cmbResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillList(cmbResult.Text);
        }

        private async void btnCheck_Click(object sender, EventArgs e)
        {
            allResult.Clear();
            rtbResult.Clear();

            string[] ipList = txtIp.Text.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            if (ipList.Length == 0)
            {
                MessageBox.Show("Please enter at least one IP address.");
                return;
            }

            bool portGirildi = int.TryParse(txtPort.Text, out int port);

            foreach (string ip in ipList)
            {
                string temizIP = ip.Trim();

                bool pingBasarili = await PingCheck(temizIP);

                string pingSonuc = pingBasarili
                    ? "Ping: Success"
                    : "Ping: Failed";

                string portSonuc = "";

                if (portGirildi)
                {
                    bool portAcik = await PortCheck(temizIP, port);

                    portSonuc = portAcik
                        ? $" | Port {port}: Open"
                        : $" | Port {port}: Close";
                }

                string finalSatir = $"{temizIP} | {pingSonuc}{portSonuc}";

                allResult.Add(finalSatir);
            }

            cmbResult.SelectedIndex = 1;
            FillList("All");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            allResult.Clear();
            rtbResult.Clear();
            txtPort.Clear();
            txtIp.Clear();
            cmbResult.SelectedIndex = 0;
        }
    }
}
