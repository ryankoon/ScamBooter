using System.Drawing;
using System.Windows;
using System.Windows.Forms;

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
            nIcon.Icon = new Icon(@"Resources\SB.ico");
            InitializeComponent();
            RemoteInputDetection rid = new RemoteInputDetection();
            rid.RegisterHooks();
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

        private void Test_Remote(object sender, RoutedEventArgs e)
        {
            RemoteConnectionDetection rce = new RemoteConnectionDetection();
            bool isRemoteConnection = rce.IsRemoteConnectionActive();

            if (isRemoteConnection)
            {
                TriggerNotification("Remote Connection Detected", "unsafe", 5000);
            }
            else
            {
                TriggerNotification("No Remote Connections Detected", "safe", 5000);
            }
        }

    }
}
