using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsManager.WinAPI;

namespace WindowsManager
{
    public static class ScreenManager
    {
        private static List<SystemScreen> screens;

        static ScreenManager()
        {
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            UpdateDisplays();
        }

        private static void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            UpdateDisplays();
        }

        private static void UpdateDisplays()
        {
            screens = new List<SystemScreen>(Screen.AllScreens.OrderBy(screen => screen.Bounds.Left).Select(screen => new SystemScreen(screen)));

            Console.WriteLine("Screens Updated:");
            foreach (var screen in screens)
            {
                Console.WriteLine("\t" + screen);
            }
        }

        private static SystemScreen FromRawScreen(Screen query)
        {
            return screens.Single(screen => screen.RawScreen.DeviceName == query.DeviceName);
        }

        public static SystemScreen GetScreen(int index)
        {
            return screens[index];
        }

        public static int GetScreenCount()
        {
            return screens.Count;
        }

        public static SystemScreen GetWindowScreen(SystemWindow window)
        {
            return FromRawScreen(Screen.FromRectangle(window.Bounds));
        }

        public static SystemScreen GetPointScreen(Point p)
        {
            return FromRawScreen(Screen.FromPoint(p));
        }
    }

    public class SystemScreen
    {
        public readonly Screen RawScreen;
        public SystemScreen(Screen screen)
        {
            this.RawScreen = screen;
        }

        public string DeviceName { get { return RawScreen.DeviceName; } }
        public bool Primary { get { return RawScreen.Primary; } }
        public Rectangle LayoutBounds { get { return RawScreen.WorkingArea; } }
        public Rectangle Bounds { get { return RawScreen.Bounds; } }

        public override string ToString()
        {
            return DeviceName + ": " + LayoutBounds + (Primary ? " (Primary)" : "");
        }
    }
}
