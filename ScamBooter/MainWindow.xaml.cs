using ScamBooter.ProtectionComponents;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Management;

namespace ScamBooter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static NotifyIcon nIcon = new NotifyIcon();
        private bool _allowOperation = false;
        private clsGetInputID MouseHandler;
        private TerminateProcesses killProcesses = new TerminateProcesses();
        public MainWindow()
        {
            nIcon.Icon = new Icon(@"Resources\SB.ico");
            InitializeComponent();
            InitializeProcEventWatcher();
            GlobalInputDetection globalHooks = new GlobalInputDetection();
            globalHooks.RegisterHooks();
            globalHooks.MouseClick += GlobalHooks_MouseClicked;
            CheckRDPSession();
        }

        private void GlobalHooks_MouseClicked(object sender, System.EventArgs e)
        {
            Handle_Raw_Mouse_Input(sender, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TriggerNotification("Scam Attempt Detected", "Remote connections were disconnected for your protection.", 5000);
        }

        private static void TriggerNotification(string header, string message, int timeout)
        {
            ToolStripMenuItem statusMenuItem = new ToolStripMenuItem("Status");
            ToolStripMenuItem disableMenuItem = new ToolStripMenuItem("Disable");
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            nIcon.Icon = SystemIcons.Error;
            nIcon.Visible = true;
            nIcon.ShowBalloonTip(timeout, header, message, ToolTipIcon.Info);
            nIcon.ContextMenuStrip = new ContextMenuStrip();
            nIcon.ContextMenuStrip.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[] {
                statusMenuItem, disableMenuItem, exitMenuItem
            });
        }

        private void CheckRDPSession()
        {
            RemoteConnectionDetection rce = new RemoteConnectionDetection();
            bool isRemoteConnection = rce.IsRemoteConnectionActive();

            if (isRemoteConnection)
            {
                TriggerNotification("Remote Connection Detected", "unsafe", 5000);
            }
            else
            {
                Debug.Print("No RDP session detected.");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MouseHandler = new clsGetInputID(new WindowInteropHelper(this).Handle);
            ComponentDispatcher.ThreadFilterMessage += ComponentDispatcher_ThreadFilterMessage;
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            ComponentDispatcher.ThreadFilterMessage -= ComponentDispatcher_ThreadFilterMessage;
        }

        void ComponentDispatcher_ThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            handled = false;
            if (MouseHandler != null)
            {
                int HardID = MouseHandler.GetDeviceID(msg);

                if (HardID > 0)
                {
                    _allowOperation = true;
                }
                else if (HardID == 0)
                {
                    _allowOperation = false;
                }
            }

        }

        public void Handle_Raw_Mouse_Input(object sender, RoutedEventArgs e)
        {
            if (_allowOperation)
            {
                Debug.Print("Local Input");
            }
            else
            {
                killProcesses.KillRemoteTools();
                TriggerNotification("Unsafe Remote Input", "DANGER", 5000);
            }
        }

        private static void InitializeProcEventWatcher()
        {
            WqlEventQuery query = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\"");
            ManagementEventWatcher watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += new EventArrivedEventHandler(EventArrived);
            watcher.Start();
            Debug.WriteLine("Event watcher has been started ...");
        }

        static void EventArrived(object sender, EventArrivedEventArgs e)
        {
            string instanceName = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"].ToString().ToLower();
            switch (instanceName)
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
