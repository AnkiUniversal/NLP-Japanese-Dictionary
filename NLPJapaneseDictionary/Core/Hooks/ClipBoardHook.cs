using NLPJapaneseDictionary.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace NLPJapaneseDictionary.Core.Hooks
{
    public sealed class ClipBoardHook : IDisposable
    {
        const int WM_DRAWCLIPBOARD = 0x0308;
        const int WM_CHANGECBCHAIN = 0x030D;
        const int WM_CLIPBOARDUPDATE = 0x031D;

        private System.Timers.Timer timeOutTimer = new System.Timers.Timer(100);
        private NotificationForm clipboardNotify;

        public event RoutedEventHandler ClipboardCopyFinished;

        public ClipBoardHook()
        {
            timeOutTimer.AutoReset = false;
            timeOutTimer.Enabled = false;
            timeOutTimer.Elapsed += OnTimeOutTimerElapsed;

            clipboardNotify = new NotificationForm();
            clipboardNotify.OnClipBoardChanged += OnClipBoardChanged;

        }

        private void OnTimeOutTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            getCopyValue = false;
        }

        ~ClipBoardHook()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (timeOutTimer != null)
                timeOutTimer.Close();
            if (clipboardNotify != null)
                clipboardNotify.UnRegiserFromClipboard();
        }

        private bool getCopyValue = false;
        public void CopyFromActiveProgram()
        {
            getCopyValue = true;
            System.Windows.Forms.SendKeys.SendWait("^c");
            timeOutTimer.Start();
        }

        private void OnClipBoardChanged(object sender, EventArgs e)
        {
            ClipBoardChangedHandle();
        }

        private void ClipBoardChangedHandle()
        {
            if (getCopyValue && System.Windows.Clipboard.ContainsText())
            {
                timeOutTimer.Stop();
                getCopyValue = false;
                var selectedText = System.Windows.Clipboard.GetText();
                ClipboardCopyFinished?.Invoke(selectedText, null);
            }
        }

        private class NotificationForm : Form
        {
            public event EventHandler OnClipBoardChanged;
            public NotificationForm()
            {
                NativeMethods.SetParent(Handle, NativeMethods.HWND_MESSAGE);
                UnRegiserFromClipboard();
                if (!NativeMethods.AddClipboardFormatListener(Handle))
                    System.Windows.Forms.MessageBox.Show("Unable to listen to clipboard. Please close and open the program again.", "Error!");
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_CLIPBOARDUPDATE)
                {
                    OnClipBoardChanged?.Invoke(null, null);
                }
                base.WndProc(ref m);
            }

            public void UnRegiserFromClipboard()
            {
                NativeMethods.RemoveClipboardFormatListener(Handle);
            }
        }
    }
}
