using System;
using System.Text;

namespace DesktopIconLayoutManager.Core.Win32
{
    public static class DesktopListView
    {
        /// <summary>
        /// Reliably gets the handle to the Desktop SysListView32 window across Windows 7, 10, and 11.
        /// Handles WorkerW parent window fallback when desktop wallpaper slideshow/themes are active.
        /// </summary>
        public static IntPtr GetHandle()
        {
            IntPtr progman = NativeMethods.FindWindow("Progman", "Program Manager");
            IntPtr shellView = NativeMethods.FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);
            IntPtr listView = IntPtr.Zero;

            if (shellView != IntPtr.Zero)
            {
                listView = NativeMethods.FindWindowEx(shellView, IntPtr.Zero, "SysListView32", null);
            }

            if (listView == IntPtr.Zero)
            {
                // Fallback for Windows 10/11 WorkerW logic
                NativeMethods.EnumWindows((hWnd, lParam) =>
                {
                    StringBuilder className = new StringBuilder(256);
                    NativeMethods.GetClassName(hWnd, className, className.Capacity);

                    if (className.ToString() == "WorkerW")
                    {
                        IntPtr childShellView = NativeMethods.FindWindowEx(hWnd, IntPtr.Zero, "SHELLDLL_DefView", null);
                        if (childShellView != IntPtr.Zero)
                        {
                            listView = NativeMethods.FindWindowEx(childShellView, IntPtr.Zero, "SysListView32", null);
                            return false; // Found, stop enumeration
                        }
                    }
                    return true; // Keep enumerating
                }, IntPtr.Zero);
            }

            return listView;
        }

        /// <summary>
        /// Checks if Auto-Arrange (Simgeleri otomatik düzenle) is enabled on the Desktop.
        /// </summary>
        public static bool IsAutoArrangeEnabled(IntPtr listViewHandle)
        {
            if (listViewHandle == IntPtr.Zero) return false;
            long style = (long)NativeMethods.GetWindowLongPtr(listViewHandle, NativeMethods.GWL_STYLE);
            return (style & NativeMethods.LVS_AUTOARRANGE) != 0;
        }

        /// <summary>
        /// Toggles or disables Auto-Arrange on the desktop.
        /// </summary>
        public static void SetAutoArrange(IntPtr listViewHandle, bool enable)
        {
            if (listViewHandle == IntPtr.Zero) return;
            long style = (long)NativeMethods.GetWindowLongPtr(listViewHandle, NativeMethods.GWL_STYLE);
            if (enable)
            {
                style |= NativeMethods.LVS_AUTOARRANGE;
            }
            else
            {
                style &= ~NativeMethods.LVS_AUTOARRANGE;
            }
            NativeMethods.SetWindowLongPtr(listViewHandle, NativeMethods.GWL_STYLE, (IntPtr)style);
        }
    }
}
