using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ScamBooter.ProtectionComponents
{
    class TerminateProcesses
    {
        string[] processNames;
        public TerminateProcesses()
        {
            processNames = new string[] { "teamviewer", "logmein", "citrix", "gotoassist", "anydesk", "ammy", "supremohelper", "supremo", "cmd" };
        }

        public void KillRemoteTools()
        {
            Debug.Print("Killing remote tools process...");

            foreach (string processName in processNames)
            {
                KillProcessContainingName(processName);
            }
        }

        private void KillProcessContainingName(string processName)
        {
            List<Process> processes = Process.GetProcesses().ToList();

            // Search for process name
            Process.GetProcesses().ToList()
                .Where(x =>
                x.ProcessName.ToLower().Contains(processName)
                )
                .ToList()
                .ForEach(x =>
                {
                    try
                    {
                        Debug.Print("Killing " + x.ProcessName);
                        x.Kill();
                    }
                    catch (Exception e)
                    {
                        //Debug.Print(e.Message);
                    }
                });



            //Countermeasure for renamed executables by searching in metadata
            foreach (Process process in processes)
            {
                bool terminate = false;
                string processFilename = GetExecutablePath(process);
                if (processFilename.Length > 0)
                {
                    string processDescription = null;

                    //Skip Windows processes
                    if (!processFilename.ToLower().Contains("system32"))
                    {
                        try
                        {
                            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(processFilename);
                            string fileDescription = null;

                            if (versionInfo != null)
                            {
                                fileDescription = versionInfo.FileDescription;
                            }

                            if (fileDescription != null)
                            {
                                processDescription = fileDescription.ToLower();
                            }

                        }
                        catch (Exception e)
                        {
                            Debug.Print("Unable to get file description for " + processFilename);
                        }
                    }

                    foreach (string processMatcher in processNames)
                    {
                        if (processDescription != null && processDescription.Contains(processMatcher))
                        {
                            terminate = true;
                            Debug.Print("Will terminate " + processDescription);
                        }
                    }

                    if (terminate)
                    {
                        try
                        {
                            Debug.Print("Killing " + processDescription);
                            process.Kill();
                        }
                        catch (Exception e)
                        {
                            Debug.Print(e.Message);
                        }
                    }
                }
            }

        }

        //Taken from http://www.aboutmycode.com/net-framework/how-to-get-elevated-process-path-in-net/
        //With a minor modification to return an empty string instead of an exception
        [Flags]
        private enum ProcessAccessFlags : uint
        {
            PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000
        }

        private static string GetExecutablePath(Process Process)
        {
            //If running on Vista or later use the new function
            if (Environment.OSVersion.Version.Major >= 6)
            {
                return GetExecutablePathAboveVista(Process.Id);
            }

            return Process.MainModule.FileName;
        }

        private static string GetExecutablePathAboveVista(int ProcessId)
        {
            var buffer = new StringBuilder(1024);
            IntPtr hprocess = OpenProcess(ProcessAccessFlags.PROCESS_QUERY_LIMITED_INFORMATION,
                                          false, ProcessId);
            if (hprocess != IntPtr.Zero)
            {
                try
                {
                    int size = buffer.Capacity;
                    if (QueryFullProcessImageName(hprocess, 0, buffer, out size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hprocess);
                }
            }
            //throw new Win32Exception(Marshal.GetLastWin32Error());
            return "";
        }

        [DllImport("kernel32.dll")]
        private static extern bool QueryFullProcessImageName(IntPtr hprocess, int dwFlags,
                       StringBuilder lpExeName, out int size);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess,
                       bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);
    }
}
