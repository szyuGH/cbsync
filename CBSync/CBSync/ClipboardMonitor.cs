using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace CBSync
{
    public sealed class ClipboardMonitor : IDisposable
    {
        const int WM_DRAWCLIPBOARD = 0x308;
        const int WM_CHANGECBCHAIN = 0x30D;

        private IntPtr nextClipboardViewer;
        private IntPtr handle;
        
        public ClipboardMonitor(IntPtr handle)
        {
            this.handle = handle;
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)handle);
            HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WndProc));
        }

        public void Dispose()
        {
            ChangeClipboardChain(handle, nextClipboardViewer);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    DisplayClipboard();
                    SendMessage(nextClipboardViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == nextClipboardViewer)
                        nextClipboardViewer = lParam;
                    else
                        SendMessage(nextClipboardViewer, msg, wParam, lParam);
                    break;                    
            }
            return IntPtr.Zero;
        }

        private void DisplayClipboard()
        {
            Console.WriteLine(Clipboard.GetText());
        }

        [DllImport("User32.dll")]
        private static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
    }
}
