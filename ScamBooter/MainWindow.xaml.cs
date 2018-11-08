using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;

namespace ScamBooter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NotifyIcon nIcon = new NotifyIcon();
        public MainWindow()
        {
            nIcon.Icon = new Icon(@"SB.ico");
            InitializeComponent();
            lookForExcel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ToolStripMenuItem statusMenuItem = new ToolStripMenuItem("Status");
            ToolStripMenuItem disableMenuItem = new ToolStripMenuItem("Disable");
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            nIcon.Visible = true;
            nIcon.ShowBalloonTip(5000, "Scam Attempt Detected", "Remote connections were disconnected for your protection.", ToolTipIcon.Info);
            nIcon.ContextMenuStrip = new ContextMenuStrip();
            nIcon.ContextMenuStrip.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[] {
                statusMenuItem, disableMenuItem, exitMenuItem
            });

            Process[] processlist = Process.GetProcesses();
            foreach (Process theprocess in processlist)
            {
                //Debug.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
                if (theprocess.ProcessName == "ScamBooter")
                {
                    Debug.WriteLine("ScamBooter is running!");
                    break;
                }
            }

        }

        private static void lookForExcel()
        {
            WqlEventQuery query = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\"");
            ManagementEventWatcher watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            watcher.Start();
            Debug.WriteLine("Event watcher has been started ...");
            //Console.ReadLine();
            //watcher.Stop();
        }

        static void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string instanceName = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"].ToString().ToLower();
            switch(instanceName)
            {
                case "cmd.exe":
                    Debug.WriteLine("Command prompt has been started ...");
                    break;
                case "eventvwr.msc":
                    Debug.WriteLine("Event viewer has been started ...");
                    break;
                case "mmc.exe":
                    Debug.WriteLine("Management console has been started ...");
                    break;
                default:
                    break;
            }
            Debug.WriteLine(instanceName);
        }
    }
}
