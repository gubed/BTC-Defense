using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BTC_Defense
{
    /// <summary>
    /// Latest Update: 12/13/16
    /// Copyright:     Copyright 2016 CajunCode, LLC
    /// Disclaimer:    This software is provided "AS IS" without any warrenties or guarantees.
    /// Credits:       http://stackoverflow.com/questions/621577/clipboard-event-c-sharp/26327002#26327002
    /// </summary>

    public partial class frmMain : Form
    {

        private string lastAddr;
        private string currAddr;
        private DateTime lastChange;
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        #region " Form "
        public frmMain()
        {
            InitializeComponent();
            lastChange = DateTime.MinValue;
            notif.Icon = new Icon(SystemIcons.Application, 40, 40);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            AddClipboardFormatListener(Handle);
            ProcessData(Clipboard.GetText());
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_CLIPBOARDUPDATE)
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    string text = (string)iData.GetData(DataFormats.Text);
                    ProcessData(text);
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveClipboardFormatListener(this.Handle);
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
                this.Hide();

            else if (FormWindowState.Normal == this.WindowState)
                notif.Visible = false;
        }

#endregion

        private void ProcessData(string data)
        {
            data = data.Trim();
            Console.WriteLine(data);
            if (!Validator.ValidateBitcoinAddress(data)) return;

            Console.WriteLine("ADDR: " + data);
            if (string.IsNullOrEmpty(lastAddr))
            {
                lastAddr = data;
                currAddr = data;

                lPrevious.Text = data;
                lPreviousTime.Text = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");

                lCurrent.Text = data;
                lCurrentTime.Text = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
                lastChange = DateTime.Now;
            }
            else
            {
                lastAddr = currAddr;
                currAddr = data;
                if (lastAddr != currAddr)
                {
                    DateTime prev = lastChange;
                    lPrevious.Text = lastAddr;
                    lPreviousTime.Text = prev.ToString("MM/dd/yyyy hh:mm:ss tt");

                    lCurrent.Text = data;
                    lCurrentTime.Text = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss tt");
                    lastChange = DateTime.Now;
                }
            }

            if (lastChange != DateTime.MinValue)
            {
                if ((DateTime.Now - lastChange).TotalSeconds <= 10 && lastAddr != currAddr) // Last address copy was less than 10 seconds ago
                {
                    lCurrent.ForeColor = Color.Red;
                    Notify();
                }
            }
        }

        private void Notify()
        {
            notif.Visible = true;
            notif.BalloonTipIcon = ToolTipIcon.Warning;
            notif.BalloonTipTitle = "BTC Defense";
            notif.BalloonTipText = "Your clipboard bitcoin address was swapped!";
            notif.ShowBalloonTip(3000);
        }


    }
}
