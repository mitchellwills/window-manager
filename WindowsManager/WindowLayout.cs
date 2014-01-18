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
    public interface WindowLayout
    {
        void Apply(SystemWindow window);
        bool IsApplied(SystemWindow window);
        Point? TransformFromCurrent(Point point, SystemWindow window);
    }

    public interface ScreenWindowLayout: WindowLayout
    {
        void Apply(SystemWindow window, SystemScreen screen);
        bool IsApplied(SystemWindow window, SystemScreen screen);
        Point? TransformFromCurrent(Point point, SystemWindow window, SystemScreen screen);
    }

    static class WindowLayoutUtil
    {
        public static Point? TransformPointFrom(Point original, Rectangle src, Rectangle dst)
        {
            double xP = (double)(original.X - src.X) / src.Width;
            double yP = (double)(original.Y - src.Y) / src.Height;
            int newX = (int)(dst.Left + dst.Width * xP);
            int newY = (int)(dst.Top + dst.Height * yP);
            return new Point(newX, newY);
        }

        public static ScreenWindowLayout CurrentScreenLayout(SystemWindow window)
        {
            return WindowLayouts.ALL_SCREEN_LAYOUTS.Where(layout => layout.IsApplied(window)).DefaultIfEmpty(KeepBoundsWindowLayout.INSTANCE).Single();
        }
    }

    public abstract class AbstractScreenWindowLayout : ScreenWindowLayout
    {
        public void Apply(SystemWindow window)
        {
            Apply(window, ScreenManager.GetWindowScreen(window));
        }
        public bool IsApplied(SystemWindow window)
        {
            return IsApplied(window, ScreenManager.GetWindowScreen(window));
        }
        public Point? TransformFromCurrent(Point point, SystemWindow window)
        {
            return TransformFromCurrent(point, window, ScreenManager.GetWindowScreen(window));
        }

        public abstract void Apply(SystemWindow window, SystemScreen screen);
        public abstract bool IsApplied(SystemWindow window, SystemScreen screen);
        public abstract Point? TransformFromCurrent(Point point, SystemWindow window, SystemScreen screen);
    }

    public abstract class AbstractBoundedScreenWindowLayout : AbstractScreenWindowLayout
    {
        public override void Apply(SystemWindow window, SystemScreen screen)
        {
            window.Bounds = CalcBounds(window, screen);
        }
        public override bool IsApplied(SystemWindow window, SystemScreen screen)
        {
            return window.Bounds == CalcBounds(window, screen);
        }
        public override Point? TransformFromCurrent(Point point, SystemWindow window, SystemScreen screen)
        {
            return WindowLayoutUtil.TransformPointFrom(point, window.Bounds, CalcBounds(window, screen));
        }

        protected abstract Rectangle CalcBounds(SystemWindow window, SystemScreen screen);
    }
    public class ScreenPercentageWindowLayout : AbstractBoundedScreenWindowLayout
    {
        double left;
        double top;
        double width;
        double height;
        public ScreenPercentageWindowLayout(double left, double top, double width, double height)
        {
            this.left = left;
            this.top = top;
            this.width = width;
            this.height = height;
        }

        protected override Rectangle CalcBounds(SystemWindow window, SystemScreen screen)
        {
            int leftPx = (int)(screen.LayoutBounds.Left + screen.LayoutBounds.Width * left);
            int topPx = (int)(screen.LayoutBounds.Top + screen.LayoutBounds.Height * top);
            int widthPx = (int)(screen.LayoutBounds.Width * width);
            int heightPx = (int)(screen.LayoutBounds.Height * height);
            return new Rectangle(leftPx, topPx, widthPx, heightPx);
        }
        public override string ToString()
        {
            return "ScreenPercentageWindowLayout[left=" + left + ", top=" + top + ", width=" + width + ", height=" + height + "]";
        }
    }
    public class KeepBoundsWindowLayout : AbstractBoundedScreenWindowLayout
    {
        public static readonly KeepBoundsWindowLayout INSTANCE = new KeepBoundsWindowLayout();
        private KeepBoundsWindowLayout() { }
        protected override Rectangle CalcBounds(SystemWindow window, SystemScreen screen)
        {
            var windowScreenLocation = ScreenManager.GetWindowScreen(window).LayoutBounds.Location;
            var zeroOffsetLocation = new Point(-windowScreenLocation.X, -windowScreenLocation.Y);
            var newBounds = window.Bounds;
            newBounds.Offset(zeroOffsetLocation);
            newBounds.Offset(screen.Bounds.Location);
            newBounds.Intersect(screen.LayoutBounds);
            return newBounds;
        }
    }

    public class ScreenBoundsWindowLayout : AbstractBoundedScreenWindowLayout
    {
        public static readonly ScreenBoundsWindowLayout INSTANCE = new ScreenBoundsWindowLayout();
        private ScreenBoundsWindowLayout() { }
        protected override Rectangle CalcBounds(SystemWindow window, SystemScreen screen)
        {
            return screen.LayoutBounds;
        }
    }

    class MaximizeWindowLayout : AbstractScreenWindowLayout
    {
        public static readonly MaximizeWindowLayout INSTANCE = new MaximizeWindowLayout();
        private MaximizeWindowLayout() { }

        public override void Apply(SystemWindow window, SystemScreen screen)
        {
            ScreenBoundsWindowLayout.INSTANCE.Apply(window, screen);
            window.ShowWindow(ShowWindowCommands.Maximize);
        }
        public override bool IsApplied(SystemWindow window, SystemScreen screen)
        {
            return window.IsMaximizedOn(screen);
        }
        public override Point? TransformFromCurrent(Point point, SystemWindow window, SystemScreen screen)
        {
            return WindowLayoutUtil.TransformPointFrom(point, window.Bounds, screen.LayoutBounds);
        }
    }

    class MinimizeWindowLayout : WindowLayout
    {
        public static readonly MinimizeWindowLayout INSTANCE = new MinimizeWindowLayout();
        private MinimizeWindowLayout() { }

        public void Apply(SystemWindow window)
        {
            window.ShowWindow(ShowWindowCommands.Minimize);
        }
        public bool IsApplied(SystemWindow window)
        {
            return window.IsMinimized;
        }
        public Point? TransformFromCurrent(Point point, SystemWindow window)
        {
            return point;
        }
    }

    public class MoveScreenLayout: WindowLayout
    {
        public static MoveScreenLayout ByIndex(int index)
        {
            return new MoveScreenLayout(()=>ScreenManager.GetScreen(index));
        }
        private readonly Func<SystemScreen> screenSelector;
        public MoveScreenLayout(Func<SystemScreen> screenSelector)
        {
            this.screenSelector = screenSelector;
        }

        public void Apply(SystemWindow window)
        {
            WindowLayoutUtil.CurrentScreenLayout(window).Apply(window, screenSelector());
        }
        public bool IsApplied(SystemWindow window)
        {
            return WindowLayoutUtil.CurrentScreenLayout(window).IsApplied(window, screenSelector());
        }
        public Point? TransformFromCurrent(Point point, SystemWindow window)
        {
            return WindowLayoutUtil.CurrentScreenLayout(window).TransformFromCurrent(point, window, screenSelector());
        }
    }

    public static class WindowLayouts
    {
        public static ScreenWindowLayout SCREEN_TOP_LEFT = new ScreenPercentageWindowLayout(0, 0, 0.5, 0.5);
        public static ScreenWindowLayout SCREEN_TOP_RIGHT = new ScreenPercentageWindowLayout(0.5, 0, 0.5, 0.5);
        public static ScreenWindowLayout SCREEN_BOTTOM_LEFT = new ScreenPercentageWindowLayout(0, 0.5, 0.5, 0.5);
        public static ScreenWindowLayout SCREEN_BOTTOM_RIGHT = new ScreenPercentageWindowLayout(0.5, 0.5, 0.5, 0.5);

        public static ScreenWindowLayout SCREEN_LEFT = new ScreenPercentageWindowLayout(0, 0, 0.5, 1.0);
        public static ScreenWindowLayout SCREEN_RIGHT = new ScreenPercentageWindowLayout(0.5, 0, 0.5, 1.0);

        public static ScreenWindowLayout SCREEN_TOP = new ScreenPercentageWindowLayout(0, 0, 1.0, 0.5);
        public static ScreenWindowLayout SCREEN_BOTTOM = new ScreenPercentageWindowLayout(0, 0.5, 1.0, 0.5);


        public static ScreenWindowLayout SCREEN_LEFT_1_3 = new ScreenPercentageWindowLayout(0, 0, 0.33, 1.0);
        public static ScreenWindowLayout SCREEN_LEFT_2_3 = new ScreenPercentageWindowLayout(0, 0, 0.67, 1.0);
        public static ScreenWindowLayout SCREEN_RIGHT_1_3 = new ScreenPercentageWindowLayout(0.67, 0, 0.33, 1.0);
        public static ScreenWindowLayout SCREEN_RIGHT_2_3 = new ScreenPercentageWindowLayout(0.33, 0, 0.67, 1.0);

        public static ScreenWindowLayout SCREEN_TOP_1_3 = new ScreenPercentageWindowLayout(0, 0, 1.0, 0.33);
        public static ScreenWindowLayout SCREEN_TOP_2_3 = new ScreenPercentageWindowLayout(0, 0, 1.0, 0.67);
        public static ScreenWindowLayout SCREEN_BOTTOM_1_3 = new ScreenPercentageWindowLayout(0, 0.67, 1.0, 0.33);
        public static ScreenWindowLayout SCREEN_BOTTOM_2_3 = new ScreenPercentageWindowLayout(0, 0.33, 1.0, 0.67);

        public static ScreenWindowLayout SCREEN_VERTICAL_CENTER = new ScreenPercentageWindowLayout(0.33, 0, 0.34, 1.0);
        public static ScreenWindowLayout SCREEN_HORIZONTAL_CENTER = new ScreenPercentageWindowLayout(0, 0.33, 1.0, 0.34);

        public static ScreenWindowLayout MAXIMIZE = MaximizeWindowLayout.INSTANCE;
        public static WindowLayout MINIMIZE = MinimizeWindowLayout.INSTANCE;


        public static ICollection<WindowLayout> ALL_LAYOUTS = new HashSet<WindowLayout>()
        {
            SCREEN_TOP_LEFT, SCREEN_TOP_RIGHT, SCREEN_BOTTOM_LEFT, SCREEN_BOTTOM_RIGHT,
            SCREEN_LEFT, SCREEN_RIGHT,
            SCREEN_TOP, SCREEN_BOTTOM,
            SCREEN_LEFT_1_3, SCREEN_LEFT_2_3, SCREEN_RIGHT_1_3, SCREEN_RIGHT_2_3,
            SCREEN_TOP_1_3, SCREEN_TOP_2_3, SCREEN_BOTTOM_1_3, SCREEN_BOTTOM_2_3,
            SCREEN_VERTICAL_CENTER, SCREEN_HORIZONTAL_CENTER,
            MAXIMIZE, MINIMIZE,
        };
        public static ICollection<ScreenWindowLayout> ALL_SCREEN_LAYOUTS = ALL_LAYOUTS.OfType<ScreenWindowLayout>().ToList();
    }
}
