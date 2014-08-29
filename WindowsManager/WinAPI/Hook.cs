using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace WindowsManager.WinAPI
{
    public class HookManager
    {
        public static KeyboardHook RegisterLLKeyboardHook()
        {
            return new KeyboardHookImpl();
        }
        public static MouseHook RegisterLLMouseHook()
        {
            return new MouseHookImpl();
        }

        private class KeyboardHookImpl : KeyboardHook
        {
            private readonly HookNative.HookProc callbackInstance = null;
            public event LLKeyEvent OnKeyEvent;
            public KeyboardHookImpl()
            {
                this.callbackInstance = new HookNative.HookProc(this.Callback);//Make sure to save it so it doesn't get GCed
                IntPtr hook = HookNative.SetWindowsHookEx(HookType.WH_KEYBOARD_LL, callbackInstance, IntPtr.Zero, 0);
            }

            private IntPtr Callback(int code, IntPtr wParam, IntPtr lParam)
            {
                if (code < 0)
                {
                    return HookNative.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                }
                KBDLLHOOKSTRUCT data = new KBDLLHOOKSTRUCT();
                Marshal.PtrToStructure(lParam, data);
                WM message = (WM)wParam;
                if (OnKeyEvent != null)
                    OnKeyEvent((message & WM.KEYUP) != 0, KeyInterop.KeyFromVirtualKey((int)data.vkCode), data.scanCode);
                return HookNative.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
        }

        private class MouseHookImpl : MouseHook
        {
            private readonly HookNative.HookProc callbackInstance = null;
            public event LLMouseMoveEvent OnMouseMove;
            public event LLMouseScrollEvent OnMouseScroll;
            public event LLMouseButtonEvent OnMouseButtonEvent;
            public MouseHookImpl()
            {
                this.callbackInstance = new HookNative.HookProc(this.Callback);//Make sure to save it so it doesn't get GCed
                IntPtr hook = HookNative.SetWindowsHookEx(HookType.WH_MOUSE_LL, callbackInstance, IntPtr.Zero, 0);
            }

            private IntPtr Callback(int code, IntPtr wParam, IntPtr lParam)
            {
                if (code < 0)
                {
                    return HookNative.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                }
                MSLLHOOKSTRUCT data = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                WM message = (WM)wParam;
                switch (message)
                {
                    case WM.MOUSEMOVE:
                        if (OnMouseMove != null)
                            OnMouseMove(new Point(data.pt.X, data.pt.Y));
                        break;
                    case WM.MOUSEWHEEL:
                        if (OnMouseScroll != null)
                            OnMouseScroll(new Point(data.pt.X, data.pt.Y), GET_WHEEL_DELTA(data.mouseData), false);
                        break;
                    case WM.MOUSEHWHEEL:
                        if (OnMouseScroll != null)
                            OnMouseScroll(new Point(data.pt.X, data.pt.Y), GET_WHEEL_DELTA(data.mouseData), true);
                        break;
                    case WM.LBUTTONDOWN:
                        if (OnMouseButtonEvent != null)
                            OnMouseButtonEvent(new Point(data.pt.X, data.pt.Y), true, MouseButton.LEFT);
                        break;
                    case WM.LBUTTONUP:
                        if (OnMouseButtonEvent != null)
                            OnMouseButtonEvent(new Point(data.pt.X, data.pt.Y), false, MouseButton.LEFT);
                        break;
                    case WM.MBUTTONDOWN:
                        if (OnMouseButtonEvent != null)
                            OnMouseButtonEvent(new Point(data.pt.X, data.pt.Y), true, MouseButton.MIDDLE);
                        break;
                    case WM.MBUTTONUP:
                        if (OnMouseButtonEvent != null)
                            OnMouseButtonEvent(new Point(data.pt.X, data.pt.Y), false, MouseButton.MIDDLE);
                        break;
                    case WM.RBUTTONDOWN:
                        if (OnMouseButtonEvent != null)
                            OnMouseButtonEvent(new Point(data.pt.X, data.pt.Y), true, MouseButton.RIGHT);
                        break;
                    case WM.RBUTTONUP:
                        if (OnMouseButtonEvent != null)
                            OnMouseButtonEvent(new Point(data.pt.X, data.pt.Y), false, MouseButton.RIGHT);
                        break;
                    default:
                        Console.WriteLine(message + " - " + data.pt.X + " - " + data.pt.Y + " - " + data.mouseData + " - " + data.flags);
                        break;
                }
                return HookNative.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
            }
            private static short GET_WHEEL_DELTA(int data)
            {
                return (short)((((long)data) >> 16) & 0xffff);
            }
            private static MSLLHOOKSTRUCTKeyState GET_KEYSTATE(int data)
            {
                Console.WriteLine(data);
                return (MSLLHOOKSTRUCTKeyState)(data & 0xffff);
            }
        }
    }
    public delegate void LLKeyEvent(bool keyup, Key vkCode, uint scanCode);
    public interface KeyboardHook
    {
        event LLKeyEvent OnKeyEvent;
    }


    public enum MouseButton
    {
        LEFT, MIDDLE, RIGHT,
    }
    public delegate void LLMouseMoveEvent(Point pt);
    public delegate void LLMouseScrollEvent(Point pt, short scroll, bool horizontal);
    public delegate void LLMouseButtonEvent(Point pt, bool pressed, MouseButton button);
    public interface MouseHook
    {
        event LLMouseMoveEvent OnMouseMove;
        event LLMouseScrollEvent OnMouseScroll;
        event LLMouseButtonEvent OnMouseButtonEvent;
    }
}
