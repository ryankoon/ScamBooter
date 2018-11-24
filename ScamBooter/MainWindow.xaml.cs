using ScamBooter.ProtectionComponents;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Management;
using System.Windows.Automation;
using System.Runtime.InteropServices;

namespace ScamBooter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private clsGetInputID MouseHandler;
        GlobalInputDetection globalHooks;
        RiskAssessor riskAssessor;
        public static NotifyIcon nIcon = new NotifyIcon();
        private bool _allowOperation = false;
        private TerminateProcesses killProcesses = new TerminateProcesses();
        public MainWindow()
        {
            nIcon.Icon = new Icon(@"Resources\SB.ico");
            InitializeComponent();

            RunningProcessDetection.InitializeProcEventWatcher();

            globalHooks = new GlobalInputDetection();
            riskAssessor = new RiskAssessor(globalHooks);
            riskAssessor.RiskThresholdReached += RiskAssessor_RiskThresholdReached;

            globalHooks.RegisterHooks();

            CheckRDPSession();
            Automation.AddAutomationFocusChangedEventHandler(RunningProcessDetection.OnFocusChangedHandler);
        }

        private void RiskAssessor_RiskThresholdReached(object sender, EventArgs e)
        {
            killProcesses.KillRemoteTools();
            TriggerNotification("Tech Support Scam Detected", "Certified technicians do not make unsolicited calls or notifications about your computer's health.", 10000);
            LaunchBrowser.Launch_Browser(); // Direct user to Microsoft's page about technical support scams
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
                riskAssessor.addRemoteConnectionRisk();
            }
            else
            {
                Debug.Print("No RDP session detected.");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Hide from task switcher
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)GetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, (int)GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            //Initialize remote clicks detection
            globalHooks.MouseClick += GlobalHooks_MouseClicked;
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
                //Debug.Print("Local Input");
            }
            else
            {
                riskAssessor.addRemoteConnectionRisk();
            }
        }


        // Taken from https://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher
        #region Hide From task Switcher (PInvoke)
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion
    }
}
