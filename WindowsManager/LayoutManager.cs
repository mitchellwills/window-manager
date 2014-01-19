using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using WindowsManager.WinAPI;

namespace WindowsManager
{
    public class LayoutManager
    {
        private readonly HotkeyManager hotkeyManager;
        public LayoutManager(HotkeyManager hotkeyManager)
        {
            this.hotkeyManager = hotkeyManager;
        }

        public LayoutShortcutBuilder Cycle(params WindowLayout[] layouts)
        {
            return new LayoutShortcutBuilder(this, layouts);
        }
        public LayoutShortcutBuilder Apply(WindowLayout layout)
        {
            return new LayoutShortcutBuilder(this, layout);
        }


        public class LayoutShortcutBuilder
        {
            private readonly WindowLayout[] layouts;
            private readonly LayoutManager layoutManager;

            public LayoutShortcutBuilder(LayoutManager layoutManager, params WindowLayout[] layouts)
            {
                this.layoutManager = layoutManager;
                this.layouts = layouts;
                if (layouts.Length == 0)
                    throw new ArgumentException("Must specify at least one layout");
            }

            public void On(ModifierKeys modifierKeys, Key key)
            {
                Hotkey hotkey = layoutManager.hotkeyManager.CreateHotkey(modifierKeys, key);
                hotkey.OnPressed += hotkey_OnPressed;
            }

            void hotkey_OnPressed(Hotkey hotkey)
            {
                SystemWindow window = WindowManager.GetForegroundWindow();
                WindowLayout layout = layouts.SkipWhile(l => !l.IsApplied(window)).Skip(1).FirstOrDefault();
                if (layout == null)
                    layout = layouts.First();

                if (!layout.IsApplied(window))
                {
                    Point oldMousePosition = System.Windows.Forms.Cursor.Position;
                    if (window.Bounds.Contains(oldMousePosition))
                    {
                        Point? newPoint = layout.TransformFromCurrent(oldMousePosition, window);
                        if (newPoint.HasValue)
                            System.Windows.Forms.Cursor.Position = newPoint.Value;
                    }
                    layout.Apply(window);
                }
            }
        }
    }

}
