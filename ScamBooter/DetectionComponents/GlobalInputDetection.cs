using GlobalLowLevelHooks;
using System;
using System.Diagnostics;

namespace ScamBooter
{
    public class GlobalInputDetection
    {
        MouseHook mouseHook;
        KeyboardHook keyboardHook;

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
            keyboardHook.KeyDown += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyDown);
            keyboardHook.KeyUp += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);
            keyboardHook.Install();

            // Remove handlers on application close
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);

            return keyboardHook;
        }

        private void MouseHook_LeftButtonUp(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            OnMouseClick(null);
        }
        protected virtual void OnMouseClick(EventArgs e)
        {
            MouseClick?.Invoke(this, e);
        }
        private void KeyboardHook_KeyDown(KeyboardHook.VKeys key)
        {
            Debug.Print("KeyDown V:" + key.Equals(KeyboardHook.VKeys.KEY_V));
        }

        private void KeyboardHook_KeyUp(KeyboardHook.VKeys key)
        {
            Debug.Print("KeyUp V:" + key.Equals(KeyboardHook.VKeys.KEY_V));
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            keyboardHook.KeyDown -= new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyDown);
            keyboardHook.KeyUp -= new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);
            keyboardHook.Uninstall();
            mouseHook.MouseMove -= new MouseHook.MouseHookCallback(MouseHook_LeftButtonUp);
            mouseHook.Uninstall();
            Debug.Print("Uninstalled global hooks");
        }


    }
}
