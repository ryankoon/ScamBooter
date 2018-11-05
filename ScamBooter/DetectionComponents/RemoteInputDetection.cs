using GlobalLowLevelHooks;
using System;
using System.Diagnostics;

namespace ScamBooter
{
    public class RemoteInputDetection
    {
        MouseHook mouseHook;
        KeyboardHook keyboardHook;
        public RemoteInputDetection()
        {
            mouseHook = new MouseHook();
            keyboardHook = new KeyboardHook();
        }
        // Create the Mouse Hook

        public void RegisterHooks()
        {
            // Capture mouse events
            mouseHook.MouseMove += new MouseHook.MouseHookCallback(MouseHook_MouseMove);
            mouseHook.Install();

            // Capture keyboard events
            keyboardHook.KeyDown += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyDown);
            keyboardHook.KeyUp += new KeyboardHook.KeyboardHookCallback(KeyboardHook_KeyUp);
            keyboardHook.Install();

            // Remove handlers on application close
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
        }

        private void MouseHook_MouseMove(MouseHook.MSLLHOOKSTRUCT mouseStruct)
        {
            Debug.Print("Mouse x:" + mouseStruct.pt.x.ToString());
            Debug.Print("Mouse y:" + mouseStruct.pt.y.ToString());
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
            mouseHook.MouseMove -= new MouseHook.MouseHookCallback(MouseHook_MouseMove);
            mouseHook.Uninstall();
            Debug.Print("Uninstalled global hooks");
        }


    }
}
