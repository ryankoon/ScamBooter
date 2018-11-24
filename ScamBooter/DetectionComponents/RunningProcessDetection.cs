using System.Windows.Automation;
using System.Diagnostics;
using System.Management;
using System;

namespace ScamBooter
{
    public class RunningProcessDetection
    {
        public RunningProcessDetection() { }

        public static void InitializeProcEventWatcher()
        {
            WqlEventQuery query = new WqlEventQuery("__InstanceCreationEvent", new TimeSpan(0, 0, 1), "TargetInstance isa \"Win32_Process\"");
            ManagementEventWatcher watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += new EventArrivedEventHandler(EventArrived);
            watcher.Start();
            Debug.WriteLine("Event watcher has been started ...");
        }

        public static void EventArrived(object sender, EventArrivedEventArgs e)
        {
            string instanceName = ((ManagementBaseObject)e.NewEvent["TargetInstance"])["Name"].ToString().ToLower();
            switch (instanceName)
            {
                case "cmd.exe":
                    Debug.WriteLine("Command prompt has been started ...");
                    break;
                //case "eventvwr.msc":
                //    Debug.WriteLine("Event viewer has been started ...");
                //    break;
                case "mmc.exe":
                    Debug.WriteLine("Management console has been started ...");
                    break;
                case "netstat.exe":
                    Debug.WriteLine("Netstat has been started ...");
                    break;
                // case "services.exe":
                //    Debug.WriteLine("Services has been started ...");
                //    break;
                default:
                    break;
            }
            //Debug.WriteLine("Process started", instanceName);
        }

        public static void OnFocusChangedHandler(object src, AutomationFocusChangedEventArgs args)
        {
            AutomationElement element = src as AutomationElement;
            if (element != null)
            {
                // To disable user unhandled expection when debugging 
                // https://stackoverflow.com/questions/16970642/visual-studio-not-breaking-on-user-unhandled-exceptions

                string name = element.Current.Name;
                int processId = element.Current.ProcessId;
                using (Process process = Process.GetProcessById(processId))
                {
                    if (process.ProcessName == "cmd" && name == "Command Prompt")
                        Debug.WriteLine("Command Prompt is in focus.");
                    else if (process.ProcessName == "explorer" && (name == "Open:" || name == "Cancel" || name == "Browse..." || name == "OK"))
                        Debug.WriteLine("Run Window is in focus.");
                        //Console.WriteLine("Name: {0}, ProcessName: {1} is in focus", name, process.ProcessName);
                }
            }
        }
    }
}
