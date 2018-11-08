using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ScamBooterService
{
    public partial class ScamBooterTermGuard : ServiceBase
    {
        public ScamBooterTermGuard()
        {
            InitializeComponent();

            ScamEventLog = new System.Diagnostics.EventLog();
            if(!System.Diagnostics.EventLog.SourceExists("Scam Detection Source"))
            {
                System.Diagnostics.EventLog.CreateEventSource("Scam Detection Source", "Scam Detection Log");
            }
            ScamEventLog.Source = "Scam Detection Source";
            ScamEventLog.Log = "Scam Detection Log";
        }

        private int eventId = 1;

        protected override void OnStart(string[] args)
        {
            ScamEventLog.WriteEntry("Starting termination guard");
            // Set up a timer that triggers every 5 seconds.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 5000; // 5 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
            Process[] pname = Process.GetProcessesByName("Process Name");
            if (pname.Length == 0)
            {
                System.Diagnostics.Process.Start("PathToExe.exe");
                ScamEventLog.WriteEntry("Process was terminated, restarting process", EventLogEntryType.Information, eventId++);
            }
        }

        protected override void OnStop()
        {
            ScamEventLog.WriteEntry("Stopping termination guard.");
        }

        private void ScamEventLog_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
