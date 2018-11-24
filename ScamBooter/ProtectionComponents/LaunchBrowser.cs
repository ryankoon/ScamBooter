using System.Diagnostics;

namespace ScamBooter.ProtectionComponents
{
    class LaunchBrowser
    {
        static public void Launch_Browser()
        {
            string target = "https://support.microsoft.com/en-ca/help/4013405/windows-protect-from-tech-support-scams";
            try
            {
                System.Diagnostics.Process.Start(target);
            }
            catch
                (
                 System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    Debug.WriteLine(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                Debug.WriteLine(other.Message);
            }
        }
    }
}
