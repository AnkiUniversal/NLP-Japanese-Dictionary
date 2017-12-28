/**
 * Copyright © 2017-2018 Anki Universal Team.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may
 * not use this file except in compliance with the License.  A copy of the
 * License is distributed with this work in the LICENSE.md file.  You may
 * also obtain a copy of the License from
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NLPJapaneseDictionary.Helpers
{
    public class DeviceInfo
    {
        public string DeviceName { get; set; }
        public int VerticalResolution { get; set; }
        public int HorizontalResolution { get; set; }
        public Rectangle MonitorArea { get; set; }
    }

    public static class ScreenHelper
    {
        private const int DektopVerticalResolution = 117;
        private const int DesktopHorizontalResolution = 118;

        private static List<DeviceInfo> results;

        public static List<DeviceInfo> GetMonitorsInfo()
        {
            results = new List<DeviceInfo>();
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, MonitorEnum, IntPtr.Zero);
            return results;
        }

        private static bool MonitorEnum(IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.Rect lprcMonitor, IntPtr dwData)
        {
            var monitor = new NativeMethods.MONITORINFOEX();
            monitor.Size = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
            bool success = NativeMethods.GetMonitorInfo(hMonitor, ref monitor);
            if (success)
            {
                var dc = NativeMethods.CreateDC(monitor.DeviceName, monitor.DeviceName, null, IntPtr.Zero);
                var di = new DeviceInfo
                {
                    DeviceName = monitor.DeviceName,
                    MonitorArea = new Rectangle(monitor.Monitor.left, monitor.Monitor.top, monitor.Monitor.right - monitor.Monitor.right, monitor.Monitor.bottom - monitor.Monitor.top),
                    VerticalResolution = NativeMethods.GetDeviceCaps(dc, DektopVerticalResolution),
                    HorizontalResolution = NativeMethods.GetDeviceCaps(dc, DesktopHorizontalResolution)
                };
                NativeMethods.ReleaseDC(IntPtr.Zero, dc);
                results.Add(di);
            }
            return true;
        }

        public static Bitmap CaptureScreen(System.Drawing.Point point, int width, int height)
        {
            var screenBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var bmpGraphics = Graphics.FromImage(screenBmp))
            {
                bmpGraphics.CopyFromScreen(point.X, point.Y, 0, 0, screenBmp.Size);
                return screenBmp;
            }
        }
    }
}
