using System.Windows.Forms;

namespace ScamBooter
{


    public class RemoteConnectionDetection
    {
        public RemoteConnectionDetection() { }

        public bool IsRemoteConnectionActive()
        {
            bool rdp = SystemInformation.TerminalServerSession;

            return rdp;
        }

    }
}
