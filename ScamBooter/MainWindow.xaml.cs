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
            nIcon.Icon = new Icon(@"SB.ico");
            InitializeComponent();
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
        }
    }
}
