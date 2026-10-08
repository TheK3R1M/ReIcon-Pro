using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DesktopIconLayoutManager.Core.Models;
using DesktopIconLayoutManager.Core.Win32;

namespace DesktopIconLayoutManager.Core.Engine
{
    public class IconLayoutEngine
    {
        /// <summary>
        /// Captures the current desktop icon layout and returns an IconLayoutProfile.
        /// </summary>
        public IconLayoutProfile CaptureCurrentLayout(string profileName)
        {
            IntPtr listViewHandle = DesktopListView.GetHandle();
            if (listViewHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Masaüstü (SysListView32) pencere tutamağı bulunamadı.");
            }

            int count = (int)NativeMethods.SendMessage(listViewHandle, NativeMethods.LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero);
            
            var profile = new IconLayoutProfile
            {
                Name = string.IsNullOrWhiteSpace(profileName) ? $"Düzen_{DateTime.Now:yyyyMMdd_HHmmss}" : profileName,
                CreatedAt = DateTime.Now,
                ScreenWidth = Screen.PrimaryScreen?.Bounds.Width ?? 1920,
                ScreenHeight = Screen.PrimaryScreen?.Bounds.Height ?? 1080,
                VirtualScreenLeft = SystemInformation.VirtualScreen.Left,
                VirtualScreenTop = SystemInformation.VirtualScreen.Top,
                VirtualScreenWidth = SystemInformation.VirtualScreen.Width,
                VirtualScreenHeight = SystemInformation.VirtualScreen.Height,
                MonitorCount = Screen.AllScreens.Length
            };

            if (count == 0) return profile;

            NativeMethods.GetWindowThreadProcessId(listViewHandle, out uint processId);
            IntPtr hProcess = NativeMethods.OpenProcess(
                NativeMethods.PROCESS_VM_OPERATION | NativeMethods.PROCESS_VM_READ | NativeMethods.PROCESS_VM_WRITE,
                false,
                processId);

            if (hProcess == IntPtr.Zero)
            {
                throw new UnauthorizedAccessException("Explorer sürecinin belleğine erişilemedi.");
            }

            IntPtr remotePoint = IntPtr.Zero;
            IntPtr remoteText = IntPtr.Zero;
            IntPtr remoteLvItem = IntPtr.Zero;

            try
            {
                int pointSize = Marshal.SizeOf<NativeMethods.POINT>();
                int textSize = 512; // 256 unicode characters
                int lvItemSize = Marshal.SizeOf<NativeMethods.LVITEMW>();

                remotePoint = NativeMethods.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)pointSize, NativeMethods.MEM_COMMIT, NativeMethods.PAGE_READWRITE);
                remoteText = NativeMethods.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)textSize, NativeMethods.MEM_COMMIT, NativeMethods.PAGE_READWRITE);
                remoteLvItem = NativeMethods.VirtualAllocEx(hProcess, IntPtr.Zero, (uint)lvItemSize, NativeMethods.MEM_COMMIT, NativeMethods.PAGE_READWRITE);

                IntPtr localPointBuffer = Marshal.AllocHGlobal(pointSize);
                IntPtr localTextBuffer = Marshal.AllocHGlobal(textSize);
                IntPtr localLvItemBuffer = Marshal.AllocHGlobal(lvItemSize);

                try
                {
                    for (int i = 0; i < count; i++)
                    {
                        // 1. Fetch Item Position
                        NativeMethods.SendMessage(listViewHandle, NativeMethods.LVM_GETITEMPOSITION, (IntPtr)i, remotePoint);
                        NativeMethods.ReadProcessMemory(hProcess, remotePoint, localPointBuffer, (uint)pointSize, out _);
                        var point = Marshal.PtrToStructure<NativeMethods.POINT>(localPointBuffer);

                        // 2. Fetch Item Text
                        var lvItem = new NativeMethods.LVITEMW
                        {
                            mask = NativeMethods.LVIF_TEXT,
                            iItem = i,
                            iSubItem = 0,
                            pszText = remoteText,
                            cchTextMax = 256
                        };

                        Marshal.StructureToPtr(lvItem, localLvItemBuffer, false);
                        NativeMethods.WriteProcessMemory(hProcess, remoteLvItem, localLvItemBuffer, (uint)lvItemSize, out _);

                        NativeMethods.SendMessage(listViewHandle, NativeMethods.LVM_GETITEMTEXTW, (IntPtr)i, remoteLvItem);
                        NativeMethods.ReadProcessMemory(hProcess, remoteText, localTextBuffer, (uint)textSize, out _);

                        string name = Marshal.PtrToStringUni(localTextBuffer) ?? string.Empty;

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            profile.Icons.Add(new DesktopIconItem
                            {
                                Name = name,
                                X = point.X,
                                Y = point.Y
                            });
                        }
                    }
                }
                finally
                {
                    Marshal.FreeHGlobal(localPointBuffer);
                    Marshal.FreeHGlobal(localTextBuffer);
                    Marshal.FreeHGlobal(localLvItemBuffer);
                }
            }
            finally
            {
                if (remotePoint != IntPtr.Zero) NativeMethods.VirtualFreeEx(hProcess, remotePoint, 0, NativeMethods.MEM_RELEASE);
                if (remoteText != IntPtr.Zero) NativeMethods.VirtualFreeEx(hProcess, remoteText, 0, NativeMethods.MEM_RELEASE);
                if (remoteLvItem != IntPtr.Zero) NativeMethods.VirtualFreeEx(hProcess, remoteLvItem, 0, NativeMethods.MEM_RELEASE);
                NativeMethods.CloseHandle(hProcess);
            }

            return profile;
        }

        /// <summary>
        /// Restores desktop icons to their saved positions from an IconLayoutProfile.
        /// </summary>
        public bool RestoreLayout(IconLayoutProfile profile, out string message, bool clampToVirtualScreen = true)
        {
            message = string.Empty;
            IntPtr listViewHandle = DesktopListView.GetHandle();
            if (listViewHandle == IntPtr.Zero)
            {
                message = "Masaüstü penceresi bulunamadı.";
                return false;
            }

            if (DesktopListView.IsAutoArrangeEnabled(listViewHandle))
            {
                // Auto arrange will override coordinates
                DesktopListView.SetAutoArrange(listViewHandle, false);
            }

            // Capture current desktop icons to build name -> index mapping
            IconLayoutProfile currentLayout;
            try
            {
                currentLayout = CaptureCurrentLayout("TempCheck");
            }
            catch (Exception ex)
            {
                message = $"Masaüstü mevcut durumu okunamadı: {ex.Message}";
                return false;
            }

            var currentIndexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < currentLayout.Icons.Count; i++)
            {
                string iconName = currentLayout.Icons[i].Name;
                if (!currentIndexMap.ContainsKey(iconName))
                {
                    currentIndexMap[iconName] = i;
                }
            }

            // Freeze redraw for smooth transition
            NativeMethods.SendMessage(listViewHandle, NativeMethods.WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);

            int restoredCount = 0;
            var vBounds = SystemInformation.VirtualScreen;

            try
            {
                foreach (var savedIcon in profile.Icons)
                {
                    if (currentIndexMap.TryGetValue(savedIcon.Name, out int targetIndex))
                    {
                        int targetX = savedIcon.X;
                        int targetY = savedIcon.Y;

                        if (clampToVirtualScreen)
                        {
                            // Ensure coordinates fit within currently active virtual screen dimensions
                            targetX = Math.Max(vBounds.Left, Math.Min(vBounds.Right - 32, targetX));
                            targetY = Math.Max(vBounds.Top, Math.Min(vBounds.Bottom - 32, targetY));
                        }

                        // Pack 32-bit (X, Y) into LPARAM for LVM_SETITEMPOSITION
                        IntPtr pos = (IntPtr)(((uint)targetY << 16) | ((uint)targetX & 0xFFFF));
                        NativeMethods.SendMessage(listViewHandle, NativeMethods.LVM_SETITEMPOSITION, (IntPtr)targetIndex, pos);
                        restoredCount++;
                    }
                }
            }
            finally
            {
                // Re-enable redraw & refresh desktop view
                NativeMethods.SendMessage(listViewHandle, NativeMethods.WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
                NativeMethods.SendMessage(listViewHandle, 0x000F /* WM_PAINT */, IntPtr.Zero, IntPtr.Zero);
            }

            message = $"{profile.Icons.Count} simgeden {restoredCount} tanesi başarıyla konumlandırıldı.";
            return true;
        }
    }
}
