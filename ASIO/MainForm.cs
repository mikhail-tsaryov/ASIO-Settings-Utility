using NAudio.Wave;
using NAudio.Wave.Asio;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ASIO
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // Disable Main form
            Visible = false;
            ShowInTaskbar = false;

            // Read ASIO device list from Windows Registry
            ContextMenuFill();

            // Create notification
            /*
            notifyIcon.BalloonTipText = "Program is running";
            notifyIcon.BalloonTipTitle = "ASIO Settings Utility";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(1000);
            */
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

            // Add item for rescan ASIO devices
            ToolStripMenuItem RescanMenuItem = new ToolStripMenuItem("Rescan ASIO devices");
            RescanMenuItem.Click += RescanMenuItem_Click;
            RescanMenuItem.ToolTipText = "Rescan ASIO devices in the system";
            RescanMenuItem.Font = new Font(RescanMenuItem.Font, RescanMenuItem.Font.Style | FontStyle.Bold);
            contextMenuStrip.Items.Add(RescanMenuItem);

            // Add separator
            contextMenuStrip.Items.Add("-");

            // Add item for quit program
            ToolStripMenuItem QuitMenuItem = new ToolStripMenuItem("Quit");
            QuitMenuItem.Click += QuitMenuItem_Click;
            QuitMenuItem.ToolTipText = "Quit ASIO Settings Utility";
            contextMenuStrip.Items.Add(QuitMenuItem);
        }

        // Rescan ASIO devices: read ASIO device list from Windows Registry and create notification
        private void RescanMenuItem_Click(object sender, EventArgs e)
        {
            ContextMenuFill();

            notifyIcon.BalloonTipText = "Rescan ASIO devices completed";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(1000);
        }

        // Close program
        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
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
                using (var asio = new AsioOut(SelectedDeviceName))
                {
                    asio.ShowControlPanel();
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

        // Rescan ASIO devices: read ASIO device list from Windows Registry and create notification
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ContextMenuFill();

                notifyIcon.BalloonTipText = "Rescan ASIO devices complete";
                notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon.ShowBalloonTip(1000);
            }
            
        }
    }
}
