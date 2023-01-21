using NAudio.Wave;
using NAudio.Wave.Asio;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASIO
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr window, int index, int value);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr window, int index);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public static void HideFromAltTab(IntPtr Handle)
        {
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle, GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
        }
        
        public MainForm()
        {
            InitializeComponent();

            // Disable Main form
            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.ShowInTaskbar = false;
            HideFromAltTab(Handle);

            // Read ASIO device list from Windows Registry
            ContextMenuFill();
            DeviceListFill();
        }

        private void ContextMenuFill()
        {
            // Clear all items from context menu
            contextMenuStrip.Items.Clear();

            // Read registered ASIO devices
            string[] devicesList = AsioOut.GetDriverNames();

            // If device list is not empty
            if (devicesList.Length > 0)
            {
                // Add item for each device
                foreach (var device in devicesList)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem(device);
                    menuItem.ToolTipText = "Open '" + device + "' control panel";
                    menuItem.Enabled = true;
                    menuItem.Click += OpenAsioSettings;
                    contextMenuStrip.Items.Add(menuItem);
                }
            }
            // If device list is empty
            else
            {
                // Add item 'no ASIO devices'
                ToolStripMenuItem NoDevicesMenuItem = new ToolStripMenuItem("(no ASIO devices)");
                NoDevicesMenuItem.Enabled = false;
                contextMenuStrip.Items.Add(NoDevicesMenuItem);
            }

            // Add separator
            contextMenuStrip.Items.Add("-");

            
            // Add item for open main window
            ToolStripMenuItem OpenMainWindowItem = new ToolStripMenuItem("Open ASIO devices info reader");
            OpenMainWindowItem.Click += OpenMainWindowItem_Click;
            OpenMainWindowItem.ToolTipText = "Open ASIO devices info reader window";
            OpenMainWindowItem.Font = new Font(OpenMainWindowItem.Font, OpenMainWindowItem.Font.Style | FontStyle.Bold);
            contextMenuStrip.Items.Add(OpenMainWindowItem);
            

            // Add item for rescan ASIO devices
            ToolStripMenuItem RescanMenuItem = new ToolStripMenuItem("Rescan ASIO devices");
            RescanMenuItem.Click += RescanMenuItem_Click;
            RescanMenuItem.ToolTipText = "Rescan ASIO devices in the system";
            contextMenuStrip.Items.Add(RescanMenuItem);

            // Add separator
            contextMenuStrip.Items.Add("-");

            // Add item for quit program
            ToolStripMenuItem QuitMenuItem = new ToolStripMenuItem("Quit");
            QuitMenuItem.Click += QuitMenuItem_Click;
            QuitMenuItem.ToolTipText = "Quit ASIO Settings Utility";
            contextMenuStrip.Items.Add(QuitMenuItem);
        }

        private void DeviceListFill()
        {
            // Clear all items from list
            listBox1.Items.Clear();

            // Read registered ASIO devices
            string[] devicesList = AsioOut.GetDriverNames();

            // Add items
            listBox1.Items.AddRange(devicesList);

            listBox1.SetSelected(0, true);
        }

        // Rescan ASIO devices: read ASIO device list from Windows Registry and create notification
        private void RescanMenuItem_Click(object sender, EventArgs e)
        {
            ContextMenuFill();
            DeviceListFill();

            notifyIcon.BalloonTipText = "Rescan ASIO devices completed";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(1000);
        }

        // Rescan ASIO devices: read ASIO device list from Windows Registry and create notification
        private void OpenMainWindowItem_Click(object sender, EventArgs e)
        {
            HideFromAltTab(Handle);
            this.Visible = true;
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }

        // Open ASIO settings for selected device
        private void OpenAsioSettings(object sender, EventArgs e)
        {
            // Getting the name of the selected device
            string SelectedDeviceName = sender.ToString();

            // Searching for selected device in the list and save it.
            ToolStripMenuItem SelectedItem = new ToolStripMenuItem();
            foreach (ToolStripMenuItem item in contextMenuStrip.Items)
            {
                if (item.ToString() == SelectedDeviceName)
                {
                    SelectedItem = item;
                    break;
                }
            }
                        
            try
            {
                // Trying to open the ASIO control panel
                using (var asioOutput = new AsioOut(SelectedDeviceName))
                {
                    asioOutput.ShowControlPanel();
                }
            }
            catch (Exception)
            {
                // If caught an exception
                // Deactivate the device name in the context menu list
                SelectedItem.Text = SelectedDeviceName + " (not available)";
                notifyIcon.BalloonTipText = SelectedDeviceName + " is disconnected or does not have a control panel";
                SelectedItem.Enabled = false;
                SelectedItem.ToolTipText = "";
                notifyIcon.BalloonTipIcon = ToolTipIcon.Error;
                notifyIcon.ShowBalloonTip(1000);    
            }
        }

        private void OpenAsioInfo(string name)
        {
            // Getting the name of the selected device
            string SelectedDeviceName = name;
            foreach (string item in listBox1.Items)
            {
                if (item == SelectedDeviceName)
                {
                    // Searching for selected device in the list and save it.
                    string SelectedItem = item.ToString();
                    break;
                }
            }

            try
            {
                // Trying to open the ASIO control panel
                richTextBox1.Clear();

                using (var asioOutput = new AsioOut(SelectedDeviceName))
                {

                    //AsioChannelInfo asioInfo = ;
                    richTextBox1.AppendText("Name: " + asioOutput.DriverName + "\r\n");
                    richTextBox1.AppendText("Input channels: " + asioOutput.DriverInputChannelCount.ToString() + "\r\n");
                    richTextBox1.AppendText("Output channels: " + asioOutput.DriverOutputChannelCount.ToString() + "\r\n");
                    richTextBox1.AppendText("Input latency: " + "???" + "\r\n");
                    richTextBox1.AppendText("Output latency: " + asioOutput.PlaybackLatency.ToString() + "\r\n");
                    richTextBox1.AppendText("Channel offset: " + asioOutput.ChannelOffset.ToString() + "\r\n");
                    richTextBox1.AppendText("Playback State: " + asioOutput.PlaybackState.ToString() + "\r\n");
                    //richTextBox1.AppendText("OutputWaveFormat: " + asioOutput.OutputWaveFormat.ToString() + "\r\n"); // return null

                    asioOutput.InitRecordAndPlayback(null, 2, 44100);
                    richTextBox1.AppendText("Frames per buffer (44100): " + asioOutput.FramesPerBuffer.ToString() + "\r\n");
                    richTextBox1.AppendText("\r\n");

                    richTextBox1.AppendText("Sample rate:\r\n");
                    richTextBox1.AppendText("--------------------------------------\r\n");
                    richTextBox1.AppendText("8000 Hz - " + (asioOutput.IsSampleRateSupported(8000) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("11025 Hz - " + (asioOutput.IsSampleRateSupported(11025) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("16000 Hz - " + (asioOutput.IsSampleRateSupported(16000) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("22050 Hz - " + (asioOutput.IsSampleRateSupported(22050) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("32000 Hz - " + (asioOutput.IsSampleRateSupported(32000) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("44100 Hz - " + (asioOutput.IsSampleRateSupported(44100) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("48000 Hz - " + (asioOutput.IsSampleRateSupported(48000) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("88200 Hz - " + (asioOutput.IsSampleRateSupported(88200) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("96000 Hz - " + (asioOutput.IsSampleRateSupported(96000) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("176400 Hz - " + (asioOutput.IsSampleRateSupported(176400) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("192000 Hz - " + (asioOutput.IsSampleRateSupported(192000) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("352800 Hz - " + (asioOutput.IsSampleRateSupported(352800) ? ("supported") : ("not supported")) + "\r\n");
                    richTextBox1.AppendText("384000 Hz - " + (asioOutput.IsSampleRateSupported(384000) ? ("supported") : ("not supported")) + "\r\n");
                }
            }
            catch (Exception ex)
            {
                // If caught an exception
                // Deactivate the device name in the context menu list
                richTextBox1.AppendText(ex.Message);
            }
        }

        // Rescan ASIO devices: read ASIO device list from Windows Registry and create notification
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenMainWindowItem_Click(sender, e);
            /*
            ContextMenuFill();

            notifyIcon.BalloonTipText = "Rescan ASIO devices complete";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(1000);
            */
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenAsioInfo(listBox1.SelectedItem.ToString());
        }

        // Close program
        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Visible = false;
                HideFromAltTab(Handle);
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void button_Rescan_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox1.Text = "No information";
            RescanMenuItem_Click(sender, e);
        }

        private void button_CopyInfo_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(richTextBox1.Text);
        }

        private void listBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                richTextBox1.Clear();
                richTextBox1.Text = "Reading info...";
                OpenAsioInfo(listBox1.SelectedItem.ToString());
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            richTextBox1.Text = "No information";
        }
    }

}
