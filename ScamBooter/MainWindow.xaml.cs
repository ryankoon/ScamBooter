using ScamBooter.ProtectionComponents;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Management;
using System.Windows.Automation;

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
            RunningProcessDetection.InitializeProcEventWatcher();
            GlobalInputDetection globalHooks = new GlobalInputDetection();
            globalHooks.RegisterHooks();
            globalHooks.MouseClick += GlobalHooks_MouseClicked;
            CheckRDPSession();
            Automation.AddAutomationFocusChangedEventHandler(RunningProcessDetection.OnFocusChangedHandler);
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
                LaunchBrowser.Launch_Browser(); // direct user to page about technical support scam
            }
        }
    }
}
