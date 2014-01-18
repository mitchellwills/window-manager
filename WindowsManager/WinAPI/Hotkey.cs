using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace WindowsManager.WinAPI
{
    public class HotkeyManager
    {
        private readonly IntPtr hWnd;
        private readonly Dictionary<int, HotkeyImpl> hotkeys = new Dictionary<int, HotkeyImpl>();

        public HotkeyManager(Window window)
        {
            hWnd = new WindowInteropHelper(window).Handle;
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }
        
        ~HotkeyManager()
        {
            // Don't need to do this? ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (!handled)
            {
                if (msg.message == HotkeyNative.WmHotKey)
                {
                    HotkeyImpl hotkey;
                    int id = (int)msg.wParam;
                    if (hotkeys.TryGetValue(id, out hotkey))
                    {
                        hotkey.Fire();
                        handled = true;
                    }
                }
            }
        }

        private int nextId = 0;
        public Hotkey CreateHotkey(ModifierKeys modifierKeys, Keys key)
        {
            if (key == Keys.None)
                throw new ArgumentException("Cannot create a hotkey with no key");
            int id = nextId++;
            HotkeyImpl hotkey = new HotkeyImpl(this, id, modifierKeys, key);
            hotkeys.Add(id, hotkey);
            return hotkey;
        }

        private bool RegisterHotkey(Hotkey hotkey)
        {
            return HotkeyNative.RegisterHotKey(hWnd, hotkey.Id, hotkey.ModifierKeys, hotkey.Key); ;
        }
        private bool UnregisterHotkey(Hotkey hotkey)
        {
            return HotkeyNative.UnregisterHotKey(hWnd, hotkey.Id); ;
        }



        private class HotkeyImpl: Hotkey
        {
            private readonly HotkeyManager manager;

            public int Id { get; private set; }
            public ModifierKeys ModifierKeys { get; private set; }
            public Keys Key { get; private set; }
            public event HotkeyPressed OnPressed;


            public HotkeyImpl(HotkeyManager manager, int id, ModifierKeys modifierKeys, Keys key)
            {
                this.manager = manager;
                this.Id = id;
                this.ModifierKeys = modifierKeys;
                this.Key = key;

                if (!manager.RegisterHotkey(this))
                    throw new ApplicationException("Hotkey already in use");
            }
            ~HotkeyImpl()
            {
                manager.UnregisterHotkey(this);
            }

            public void Fire()
            {
                if (OnPressed != null)
                    OnPressed(this);
            }
        }
    }

    public delegate void HotkeyPressed(Hotkey hotkey);
    public interface Hotkey
    {
        int Id { get; }
        ModifierKeys ModifierKeys { get; }
        Keys Key { get; }
        event HotkeyPressed OnPressed;
    }
}
