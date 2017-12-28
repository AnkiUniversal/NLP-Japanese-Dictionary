using NLPJapaneseDictionary.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NLPJapaneseDictionary.Core.Hooks
{
    public class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private NativeMethods.LowLevelKeyboardProc keyboardProc;
        private IntPtr hookId = IntPtr.Zero;

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;

        public Key SelectedKey { get; set; }


        private static KeyboardHook thisInstance;

        public static KeyboardHook GetInstance()
        {
            if (thisInstance == null)
            {
                thisInstance = new KeyboardHook();
            }
            return thisInstance;
        }

        private KeyboardHook()
        {
            keyboardProc = HookCallback;
            hookId = SetHook(keyboardProc);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual unsafe void Dispose(bool isTrue)
        {
            if (isTrue)
            {
                NativeMethods.UnhookWindowsHookEx(hookId);
            }
        }

        public delegate bool KeyboardPressedEventHandler(Key key);
        public event KeyboardPressedEventHandler KeyPressed;

        private IntPtr SetHook(NativeMethods.LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                                        NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool isSuppress = false;
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                var keyPressed = KeyInterop.KeyFromVirtualKey(vkCode);
                Trace.WriteLine(keyPressed);
                Trace.WriteLine("Triggering Keyboard Hook");
                isSuppress = KeyPressed(keyPressed);
            }
            if (isSuppress)
            {
                return wParam;
            }

            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public static void ActivateWindow(Window window)
        {
            var interopHelper = new WindowInteropHelper(window);
            var currentForegroundWindow = NativeMethods.GetForegroundWindow();
            var thisWindowThreadId = NativeMethods.GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);
            var currentForegroundWindowThreadId = NativeMethods.GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);
            NativeMethods.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);
            NativeMethods.SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);
            NativeMethods.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);
            window.Show();
            window.Activate();
        }


    }
}
