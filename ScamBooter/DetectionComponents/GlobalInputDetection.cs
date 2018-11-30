using GlobalLowLevelHooks;
using System;
using System.Collections;
using System.Diagnostics;
using System.Windows;

namespace ScamBooter
{
    public class GlobalInputDetection
    {

        public class SuspiciousInputArgs : EventArgs
        {
            public virtual string matcherFound { get; set; }
        }

        MouseHook mouseHook;
        KeyboardHook keyboardHook;
        RunningProcessDetection.ProcessEvents currentWindowFocus = RunningProcessDetection.ProcessEvents.OTHER_FOCUS;

        string keyInputsString = "";
        readonly ArrayList matchers = new ArrayList {
            "dir/s",
            "tree",
            "netstat",
            "virus",
            "atrisk",
            "zeusvirus",
            "infection",
            "koobface",
            "iexplorer"
            };

        ArrayList matcherDetectionHistory = new ArrayList();

        public event EventHandler<SuspiciousInputArgs> SuspiciousInput;
        public event EventHandler MouseClick;
        public GlobalInputDetection()
        {
            mouseHook = new MouseHook();
            keyboardHook = new KeyboardHook();

            RunningProcessDetection.ProcessEvent += RunningProcessDetection_ProcessEvent; ;
        }

        private void RunningProcessDetection_ProcessEvent(object sender, RunningProcessDetection.ProcessEventArgs e)
        {
            if (isWindowFocusEvent(e))
            {
                currentWindowFocus = e.ProcessEvent;
            }
        }

        private bool isWindowFocusEvent(RunningProcessDetection.ProcessEventArgs e)
        {
            return isProcessEvent(e, RunningProcessDetection.ProcessEvents.CMD_WINDOW_FOCUS) || isProcessEvent(e, RunningProcessDetection.ProcessEvents.RUN_WINDOW_FOCUS) || isProcessEvent(e, RunningProcessDetection.ProcessEvents.OTHER_FOCUS);
        }

        private bool isProcessEvent(RunningProcessDetection.ProcessEventArgs args, RunningProcessDetection.ProcessEvents processEvent)
        {
            return args.ProcessEvent.Equals(processEvent);
        }
        //Create the Mouse Hook

        public KeyboardHook RegisterHooks()
        {
            // Capture mouse events
            mouseHook.LeftButtonUp += new MouseHook.MouseHookCallback(MouseHook_LeftButtonUp);
            mouseHook.Install();

            // Capture keyboard events
            keyboardHook.KeyUp += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);
            keyboardHook.Install();

            // Remove handlers on application close
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            return keyboardHook;
        }

        public void clearKeyInputsString()
        {
            keyInputsString = "";
        }

        public bool checkBuiltInMatchers()
        {
            bool result = false;
            foreach (string matcher in matchers)
            {
                result = result || checkMatcher(matcher);
            }
            return result;
        }

        public bool checkMatcher(string matchString)
        {
            string clipboardText = "";
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                clipboardText = Clipboard.GetText(TextDataFormat.Text);
            }

            bool result = keyInputsString.Contains(matchString) || clipboardText.Contains(matchString);
            if (result)
            {
                matcherDetectionHistory.Add(matchString);
                SuspiciousInput?.Invoke(this, new SuspiciousInputArgs { matcherFound = matchString });
                clearKeyInputsString();
            }
            return result;
        }

        private void MouseHook_LeftButtonUp(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            OnMouseClick(null);
        }
        protected virtual void OnMouseClick(EventArgs e)
        {
            MouseClick?.Invoke(this, e);
        }

        private void KeyboardHook_KeyUp(KeyboardHook.VKeys key)
        {
            //Only detect keystrokes in command prompt
            if (currentWindowFocus == RunningProcessDetection.ProcessEvents.CMD_WINDOW_FOCUS)
            {
                if (key.ToString() == "OEM_2")
                {
                    keyInputsString += "/";
                    checkBuiltInMatchers();
                }
                else if (key.ToString().Contains("KEY_"))
                {
                    keyInputsString += key.ToString().ToLower().Substring(4);
                    checkBuiltInMatchers();
                }
                else if (key.ToString() == "BACK")
                {
                    if (keyInputsString.Length > 0)
                    {
                        keyInputsString = keyInputsString.Remove(keyInputsString.Length - 1);
                    }
                }
            } else
            {
                //Debug.Print("Keylogging analysis skipped for non-targetted windows.");
            }

        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            keyboardHook.KeyUp -= new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);
            keyboardHook.Uninstall();
            mouseHook.MouseMove -= new MouseHook.MouseHookCallback(MouseHook_LeftButtonUp);
            mouseHook.Uninstall();
            Debug.Print("Uninstalled global hooks");
        }
    }
}
