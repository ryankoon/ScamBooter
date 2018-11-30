using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScamBooter.ProtectionComponents
{
    public class RiskAssessor
    {
        public event EventHandler RiskThresholdReached;
        readonly int RiskThreshold = 100;
        RunningProcessDetection.ProcessEvents currentWindowFocus = RunningProcessDetection.ProcessEvents.OTHER_FOCUS; 

        HashSet<EventRisk> DetectedRisks = new HashSet<EventRisk>();
        private static Mutex mutex = new Mutex();

        public enum EventRisk
        {
            REMOTE_CONNECTION,
            COMMAND_PROMPT,
            CMD_SCAN,
            SUSPICIOUS_KEYBOARD_INPUT,
            EVENT_VIEWER,
            SYSTEM_WINDOW,
            RUN_WINDOW,
            RUN_IEXPLORER
        }
        public Dictionary<EventRisk, int> EventRiskScores = new Dictionary<EventRisk, int>()
        {
            {EventRisk.REMOTE_CONNECTION, 50},
            {EventRisk.COMMAND_PROMPT, 20},
            {EventRisk.CMD_SCAN, 20},
            {EventRisk.SUSPICIOUS_KEYBOARD_INPUT, 40},
            {EventRisk.EVENT_VIEWER, 30},
            {EventRisk.SYSTEM_WINDOW, 20},
            {EventRisk.RUN_WINDOW, 20},
            {EventRisk.RUN_IEXPLORER, 20}
        };

        public RiskAssessor(GlobalInputDetection globalHooks)
        {
            RunningProcessDetection.ProcessEvent += RunningProcessDetection_ProcessEvent;
            globalHooks.SuspiciousInput += GlobalHooks_SuspiciousInput;
        }

        public void GlobalHooks_SuspiciousInput(object sender, GlobalInputDetection.SuspiciousInputArgs e)
        {
            if (e.matcherFound == "dir/s" || e.matcherFound == "tree")
            {
                addAndAssessRisks(EventRisk.CMD_SCAN);
            }
            else if (e.matcherFound == "iexplorer")
            {
                addAndAssessRisks(EventRisk.RUN_IEXPLORER);
            } else
            {
                addAndAssessRisks(EventRisk.SUSPICIOUS_KEYBOARD_INPUT);
            }
        }

        public bool addRemoteConnectionRisk()
        {
            return addAndAssessRisks(EventRisk.REMOTE_CONNECTION);
        }

        private void RunningProcessDetection_ProcessEvent(object sender, RunningProcessDetection.ProcessEventArgs e)
        {
            if (isEventRisk(e, RunningProcessDetection.ProcessEvents.CMD_PROCESS))
            {
                addAndAssessRisks(EventRisk.COMMAND_PROMPT);
            }
            else if (isEventRisk(e, RunningProcessDetection.ProcessEvents.MANAGEMENT_CONSOLE_PROCESS))
            {
                addAndAssessRisks(EventRisk.EVENT_VIEWER);
            }
            else if (isEventRisk(e, RunningProcessDetection.ProcessEvents.RUN_WINDOW_FOCUS))
            {
                addAndAssessRisks(EventRisk.RUN_WINDOW);
            }
        }

        private bool isEventRisk(RunningProcessDetection.ProcessEventArgs args, RunningProcessDetection.ProcessEvents processEvent) {
            return args.ProcessEvent.Equals(processEvent);
        }

        public bool addAndAssessRisks(EventRisk newRisk)
        {
            AddRisk(newRisk);

            //Calculate risk score
            int riskScore = calculateRiskScore();

            //Check if threshold reached
           return isThresholdReached(riskScore);

        }

        public bool isThresholdReached(int riskScore)
        {
            bool result = false;
            if (riskScore >= RiskThreshold)
            {
                RiskThresholdReached?.Invoke(this, new EventArgs { });
                Debug.Print("Risk threshold reached: " + RiskThreshold.ToString());
                result = true;
            }
            return result;
        }

        public void AddRisk(EventRisk newRisk)
        {
            mutex.WaitOne();

            DetectedRisks.Add(newRisk);
            Debug.Print("Risk Added: " + newRisk.ToString());
            Debug.Print("Current Risks: ");

            mutex.ReleaseMutex();
        }

        public int calculateRiskScore()
        {
            mutex.WaitOne();

            int riskScore = 0;
            foreach (EventRisk detectedRisk in DetectedRisks)
            {
                riskScore += EventRiskScores[detectedRisk];
                Debug.Print(detectedRisk.ToString() + " " + "+" + EventRiskScores[detectedRisk].ToString());
            }

            mutex.ReleaseMutex();

            Debug.Print("------------------");
            Debug.Print("Current Risk Score: " + riskScore.ToString());
            return riskScore;
        }

        public HashSet<EventRisk> GetDetectedRisks()
        {
            return DetectedRisks;
        }
    }
}
