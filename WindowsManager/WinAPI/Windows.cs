using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WindowsManager.WinAPI
{
    public class WindowManager
    {
        public static SystemWindow GetForegroundWindow()
        {
            return new SystemWindow(WindowsNative.GetForegroundWindow());
        }
        public static ICollection<SystemWindow> GetAllWindows()
        {
            ICollection<SystemWindow> windows = new List<SystemWindow>();
            WindowsNative.EnumWindows((IntPtr hwnd, int lparam) =>
            {
                windows.Add(new SystemWindow(hwnd));
                return true;
            }, 0);
            return windows;
        }
    }

    public class SystemWindow
    {
        public readonly IntPtr HWND;
        public SystemWindow(IntPtr HWND)
        {
            this.HWND = HWND;
        }

        public String Title
        {
            get
            {
                int bufferSize = WindowsNative.GetWindowTextLength(HWND) + 1;//include null terminator
                StringBuilder sb = new StringBuilder(bufferSize);
                WindowsNative.GetWindowText(HWND, sb, bufferSize);
                return sb.ToString();
            }
        }

        public int ProcessId
        {
            get
            {
                uint pid = 1;//Make sure not to pass 0
                WindowsNative.GetWindowThreadProcessId(HWND, out pid);
                return (int)pid;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                RECT rect;
                WindowsNative.GetWindowRect(HWND, out rect);
                return rect;
            }
            set
            {
                ShowWindow(ShowWindowCommands.Restore);//must restore the window just in case it's maximized or minimized
                SetWindowPos(WindowsNative.HWND_TOP, value.Left, value.Top, value.Width, value.Height, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
            }
        }

        public void SetForeground()
        {
            WindowsNative.SetForegroundWindow(HWND);
        }

        public void SetWindowPos(IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags)
        {
            WindowsNative.SetWindowPos(HWND, hWndInsertAfter, X, Y, cx, cy, uFlags);
        }

        public void ShowWindow(ShowWindowCommands command)
        {
            WindowsNative.ShowWindow(HWND, command);
        }

        public WINDOWPLACEMENT GetPlacement()
        {
            WINDOWPLACEMENT placement = WINDOWPLACEMENT.Default;
            WindowsNative.GetWindowPlacement(HWND, out placement);
            return placement;
        }

        public bool IsVisible
        {
            get
            {
                return WindowsNative.IsWindowVisible(HWND);
            }
        }

        public bool IsMaximized
        {
            get
            {
                var cmd = GetPlacement().ShowCmd;
                return (cmd == ShowWindowCommands.Minimize || cmd == ShowWindowCommands.ShowMinimized || cmd == ShowWindowCommands.ShowMinNoActive);
            }
        }
        public bool IsMinimized
        {
            get
            {
                var cmd = GetPlacement().ShowCmd;
                return (cmd == ShowWindowCommands.Maximize || cmd == ShowWindowCommands.ShowMaximized);
            }
        }
        public bool IsMaximizedOn(SystemScreen screen)
        {
            return IsMaximized && ScreenManager.GetWindowScreen(this)==screen;
        }
    }
}
