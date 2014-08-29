using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsManager.WinAPI;

namespace WindowsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MouseHook mouseHook;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                mouseHook = HookManager.RegisterLLMouseHook();
                mouseHook.OnMouseScroll += (Point pt, short scroll, bool horizontal) =>
                {
                    if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                        && (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                    {
                        var screen = ScreenManager.GetPointScreen(System.Windows.Forms.Cursor.Position);

                        var allVisibleWindows = WindowManager.GetAllWindows().Where(window => window.IsVisible && window.Title.Length > 0 && window.Title != "Program Manager" && !window.IsMinimized);
                        var windowsOnScreen = allVisibleWindows.Where(window => ScreenManager.GetWindowScreen(window) == screen).OrderBy(window => window.Title);

                        SystemWindow active = WindowManager.GetForegroundWindow();
                        var next = windowsOnScreen.SkipWhile(window => window.HWND != active.HWND).Skip(1).FirstOrDefault();
                        if (next != null)
                            next.SetForeground();
                        else if (windowsOnScreen.Count() > 0)
                            windowsOnScreen.First().SetForeground();
                    }
                };


                HotkeyManager hotkeyManager = new HotkeyManager(this);
                LayoutManager layoutManager = new LayoutManager(hotkeyManager);
                layoutManager.Apply(WindowLayouts.SCREEN_TOP_LEFT).On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad7);
                layoutManager.Apply(WindowLayouts.SCREEN_TOP_RIGHT).On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad9);
                layoutManager.Apply(WindowLayouts.SCREEN_BOTTOM_LEFT).On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad1);
                layoutManager.Apply(WindowLayouts.SCREEN_BOTTOM_RIGHT).On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad3);

                layoutManager.Cycle(WindowLayouts.SCREEN_LEFT, WindowLayouts.SCREEN_LEFT_1_3, WindowLayouts.SCREEN_LEFT_2_3)
                    .On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad4);
                layoutManager.Cycle(WindowLayouts.SCREEN_RIGHT, WindowLayouts.SCREEN_RIGHT_1_3, WindowLayouts.SCREEN_RIGHT_2_3)
                    .On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad6);

                layoutManager.Cycle(WindowLayouts.SCREEN_TOP, WindowLayouts.SCREEN_TOP_1_3, WindowLayouts.SCREEN_TOP_2_3)
                    .On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad8);
                layoutManager.Cycle(WindowLayouts.SCREEN_BOTTOM, WindowLayouts.SCREEN_BOTTOM_1_3, WindowLayouts.SCREEN_BOTTOM_2_3)
                    .On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad2);

                layoutManager.Cycle(WindowLayouts.MAXIMIZE, WindowLayouts.SCREEN_VERTICAL_CENTER, WindowLayouts.SCREEN_HORIZONTAL_CENTER)
                    .On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad5);

                layoutManager.Apply(WindowLayouts.MINIMIZE).On(ModifierKeys.Windows | ModifierKeys.Alt,Key.NumPad0);

                layoutManager.Apply(MoveScreenLayout.ByIndex(0)).On(ModifierKeys.Windows | ModifierKeys.Control,Key.NumPad1);
                layoutManager.Apply(MoveScreenLayout.ByIndex(1)).On(ModifierKeys.Windows | ModifierKeys.Control,Key.NumPad2);
                layoutManager.Apply(MoveScreenLayout.ByIndex(2)).On(ModifierKeys.Windows | ModifierKeys.Control,Key.NumPad3);


                hotkeyManager.CreateHotkey(ModifierKeys.Control | ModifierKeys.Alt,Key.NumPad1).OnPressed += (hotkey) =>
                {
                    System.Windows.Forms.Cursor.Position = ScreenManager.GetScreen(0).Bounds.Center();
                };
                hotkeyManager.CreateHotkey(ModifierKeys.Control | ModifierKeys.Alt,Key.NumPad2).OnPressed += (hotkey) =>
                {
                    System.Windows.Forms.Cursor.Position = ScreenManager.GetScreen(1).Bounds.Center();
                };
                hotkeyManager.CreateHotkey(ModifierKeys.Control | ModifierKeys.Alt,Key.NumPad3).OnPressed += (hotkey) =>
                {
                    System.Windows.Forms.Cursor.Position = ScreenManager.GetScreen(2).Bounds.Center();
                };


                hotkeyManager.CreateHotkey(ModifierKeys.Control | ModifierKeys.Alt,Key.NumPad0).OnPressed += (hotkey) =>
                {
                    var screen = ScreenManager.GetPointScreen(System.Windows.Forms.Cursor.Position);

                    var allVisibleWindows = WindowManager.GetAllWindows().Where(window => window.IsVisible && window.Title.Length > 0 && window.Title != "Program Manager" && !window.IsMinimized);
                    var windowsOnScreen = allVisibleWindows.Where(window => ScreenManager.GetWindowScreen(window) == screen).OrderBy(window => window.Title);

                    SystemWindow active = WindowManager.GetForegroundWindow();
                    var next = windowsOnScreen.SkipWhile(window => window.HWND != active.HWND).Skip(1).FirstOrDefault();
                    if (next != null)
                        next.SetForeground();
                    else if (windowsOnScreen.Count() > 0)
                        windowsOnScreen.First().SetForeground();
                };
                Hide();
            };
        }
    }
}
