using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Synchronizer
{
    public partial class MainForm : Form
    {
        private SerialPort RSP1A_Port;
        private string Buffer = "";
        private SerialPort Win4Yaesu_Port;
        private string mode;

        public string pstr { get; private set; }

        public MainForm()
        {
            InitializeComponent();

            RSP1A_Port = new SerialPort("COM44", 38400, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                RtsEnable = true,
                ReadTimeout = 3000
            };
            //KeyerPort.DataReceived += new SerialDataReceivedEventHandler(Sp_DataReceived);
           RSP1A_Port.Open();

            Win4Yaesu_Port = new SerialPort("COM23", 38400, Parity.None, 8, StopBits.One)
            {
                Handshake = Handshake.None,
                RtsEnable = true,
                ReadTimeout = 3000
            };
            //Win4Yaesu_Port.DataReceived += new SerialDataReceivedEventHandler(Sp_DataReceived);
            Win4Yaesu_Port.Open();

           // timer1.Enabled = true;

        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = Win4Yaesu_Port.ReadExisting();
            Buffer += data;
            Console.WriteLine("** " + Buffer);
        }

        private void ToRadio_Click(object sender, EventArgs e)
        {
            RSP1A_Port.DiscardInBuffer();
            RSP1A_Port.DiscardOutBuffer();
            RSP1A_Port.Write("FA;");
            Thread.Sleep(500);
            string resp = RSP1A_Port.ReadExisting();
            Console.Out.WriteLine(resp);
            int l = resp.Length - 5;
            string ftext = resp.Substring(4, l);

            Console.WriteLine(ftext);

            String cmd = "FA" + ftext   +";";

            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            Win4Yaesu_Port.Write(cmd);

        }

        private void ToSDR_Click(object sender, EventArgs e)
        {
            // FE FE 00 E0 03 FD
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();

            String cmd = "FA;";

            Win4Yaesu_Port.Write(cmd);
            Thread.Sleep(1000);

            string resp;
            resp = Win4Yaesu_Port.ReadTo(";");

            Console.Out.WriteLine(resp);  // FE FE E0 00 03 00 25 52 09 00 FD => FA00009522500;

            string s;
            s = resp.Substring(2);
            Console.Out.WriteLine(s);
            s = "FA000" + s + ";";
            RSP1A_Port.DiscardInBuffer();
            RSP1A_Port.DiscardOutBuffer();
            RSP1A_Port.Write(s);
        }

        private void issueCmd(string cmd)
        {
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();

            Win4Yaesu_Port.Write(cmd);
            Thread.Sleep(20);
        }

      

        private void tuneButton_MouseDown(object sender, MouseEventArgs e)
        {
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            String cmd = "MD0;";

            Win4Yaesu_Port.Write(cmd);
            Thread.Sleep(50);

            string resp;

            mode = Win4Yaesu_Port.ReadTo(";");
            Console.Out.WriteLine(mode);
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            cmd = "PC;";

            Win4Yaesu_Port.Write(cmd);
            Thread.Sleep(50);

            resp = Win4Yaesu_Port.ReadTo(";");
            pstr = resp.Substring(2);
            Console.Out.WriteLine(pstr);
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            cmd = "PC010;";

            issueCmd(cmd);
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();

            issueCmd("MD05;");
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();

            issueCmd("MX1;");
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
        }

        private void tuneButton_MouseUp(object sender, MouseEventArgs e)
        {
            issueCmd("MX0;");
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            string cmd = mode + ";";
            issueCmd(cmd);
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();

            cmd = "PC" + pstr + ";";
            issueCmd(cmd);
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void commandButton_Click(object sender, EventArgs e)
        {
            issueCmd(commandBox.Text);
            string resp;

            resp = Win4Yaesu_Port.ReadTo(";");
            Console.WriteLine(resp);    
        }

        private void levelControl_ValueChanged(object sender, EventArgs e)
        {
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            String cmd = "VS;";

            Win4Yaesu_Port.Write(cmd);
            Thread.Sleep(50);

            String resp = Win4Yaesu_Port.ReadTo(";");
            String selectedVFO = resp.Substring(2);
            Console.Out.WriteLine(selectedVFO);


            int level = (int)levelControl.Value;
            float flevel = -((float)level/2);
            levelBox.Text = String.Format("{0:00.0}dB", flevel);
            String sign = flevel >= 0 ? "+" : "-";
            flevel = Math.Abs(flevel);
            //String fmt = "SS04{0}{1:00.0};";
            String fmt = "SS{2}4{0}{1:00.0};";
            cmd = String.Format(fmt, sign, flevel, selectedVFO);
            issueCmd(cmd);
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
            String cmd = "SS04;";

            Win4Yaesu_Port.Write(cmd);
            Thread.Sleep(1000);

            //String resp = Win4Yaesu_Port.ReadTo(";");
            String resp = Win4Yaesu_Port.ReadExisting();
            pstr = resp.Substring(2);
            Console.Out.WriteLine(pstr);
            Win4Yaesu_Port.DiscardInBuffer();
            Win4Yaesu_Port.DiscardOutBuffer();
        }
    }
}
