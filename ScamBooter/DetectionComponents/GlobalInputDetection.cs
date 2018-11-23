using GlobalLowLevelHooks;
using System;
using System.Collections;
using System.Diagnostics;

namespace ScamBooter
{
    public class GlobalInputDetection
    {

        public class SuspiciousInputArgs : EventArgs
        {
            public string matcherFound { get; set; }
        }

        MouseHook mouseHook;
        KeyboardHook keyboardHook;

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
            bool result =  keyInputsString.Contains(matchString);
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
            if (key.ToString() == "OEM_2") {
                keyInputsString += "/";
                checkBuiltInMatchers();
            }
            else if (key.ToString() != "SPACE" && key.ToString() != "RETURN")
            {
                keyInputsString += key.ToString().ToLower().Substring(4);
                checkBuiltInMatchers();
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
