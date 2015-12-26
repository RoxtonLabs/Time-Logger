using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;   //For importing DLLs
using System.Timers;    //For the timer (duh)

namespace TimeLogger
{
    public partial class MainForm : Form
    {
        Boolean running = false;
        System.Timers.Timer logTimer = new System.Timers.Timer(60000);    //Once a minute
        int minutesWorked = 0;

        //Import the LastInputInfo function
        [DllImport("User32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
        internal struct LASTINPUTINFO
        {
            public uint cbSize;
            public uint dwTime;
        }

        static uint GetLastInputTime()
        {   //Calculates the time since last input in seconds
            //Sourced from http://www.pinvoke.net/default.aspx/user32.GetLastInputInfo
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;
                idleTime = envTicks - lastInputTick;
            }
            return ((idleTime > 0) ? (idleTime / 1000) : 0);
        }

        public MainForm()
        {
            InitializeComponent();
            logTimer.Enabled = false;
            logTimer.Elapsed += new ElapsedEventHandler(logTimer_Elapsed);
        }

        void logTimer_Elapsed(object sender, ElapsedEventArgs e)
        {   //Called once a minute when the timer elapses
            if (GetLastInputTime() < 60)
            {   //If the user has done something within the last minute

                //Increase the counter
                minutesWorked += 1;

                //When we have the background worker up and working update status label
                //For now just print out
                Console.WriteLine("User last did something " + GetLastInputTime().ToString() + " seconds ago. " + minutesWorked.ToString() + " minutes worked.");
            }
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (!running)
            {   //If not running then start running
                if (minutesWorked>0)
                {
                    statusLabel.Text = minutesWorked.ToString() + " minutes worked. Running...";
                }
                else
                {
                    statusLabel.Text = "Running...";
                }
                startButton.Text = "Stop";
                running = true;
                logTimer.Enabled = true;
                logTimer.Stop();
                logTimer.Start();
            }
            else
            {
                startButton.Text = "Start";
                running = false;
                logTimer.Stop();
                logTimer.Enabled = false;
                statusLabel.Text = minutesWorked.ToString() + " minutes worked.";
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                notifyIcon.Visible = true;
                Hide();     //Hide the application from the taskbar
            }
            else
            {
                notifyIcon.Visible = false;
            }
        }

        private void notifyIcon_Click(object sender, EventArgs e)
        {
            Show();     //Restore the app to the taskbar
            WindowState = FormWindowState.Normal;    //Un-minimize the app window
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            minutesWorked = 0;
            if (running)
            {
                statusLabel.Text = "Restarted. Running...";
            }
            else
            {
                statusLabel.Text = "Restarted. Stopped.";
            }
        }
    }
}
