using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace IdleCheck
{
    internal static class Program
    {

        //Import from Windows-API -> works like Win+L for locking current user
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();

        //Windows-API: gets information about last activity / input from user (mouse or key)
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        // IMPORTANT: Frees unmanaged HICON handles created by GetHicon()
        // Without this, GDI memory leaks occur over time
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);



        //the structure, filled by GetLastInputInfo
        struct LASTINPUTINFO
        {
            public uint cbSize;     //size (has to be set!)
            public uint dwTime;     //time of last input (in ms since system boot)
        }

        //Tray icon -> small icon next to date/time (bottom right in task bar)
        static NotifyIcon trayIcon;

        //timer to check inactivity every sec
        static Timer idleTimer;

        //standard time-out for locking
        static int timeoutMinutes = 3;


        [STAThread]
        static void Main()
        {
            //standard setup for winForms
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //initializing the tray-icon
            trayIcon = new NotifyIcon
            {
                Icon = CreateProgressIcon(0, Color.Green),
                Visible = true,
                Text = "Idle Lock Timer"
            };

            //menu for right clicking (choosing timeout-timer in min from 1-10 + an Exit button
            ContextMenuStrip menu = new ContextMenuStrip();
            for (int i = 1; i <= 10; i++)
            {
                int min = i;
                menu.Items.Add($"{min} min", null, (s, e) => { timeoutMinutes = min; });
            }

            menu.Items.Add("Exit", null, (s, e) =>
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                Application.Exit();
            });


            trayIcon.ContextMenuStrip = menu;

            //Timer, activates every second
            idleTimer = new Timer { Interval = 1000 };
            idleTimer.Tick += IdleTimer_Tick;
            idleTimer.Start();

            //starts message loop without a visible form/window
            Application.Run();
        }


        /// <summary>
        /// called every second
        /// - checks idle-time
        /// - updates Tray-Icon (Progess + Color)
        /// - locks PC, if timer hits 0
        /// </summary>
        static void IdleTimer_Tick(object sender, EventArgs e)
        {
            //calc. idle time in min
            double idleMinutes = GetIdleTime() / 60000.0; // Millisec → Minutes
            
            //progress (0.0 - 1.0)
            double progress = Math.Min(1.0, idleMinutes / timeoutMinutes);

            //color depending on progress (green -> red)
            Color color = ColorFromProgress(progress);

            if (trayIcon.Icon != null)
            {
                trayIcon.Icon.Dispose();  // disposes of old Icon-Handle to prevent memory leaks
            }
            trayIcon.Icon = CreateProgressIcon(progress, color);

            //if timer hits 0 -> lock PC
            if (idleMinutes >= timeoutMinutes)
            {
                LockWorkStation();
            }
        }

        /// <summary>
        /// calc. time since last input in milli-secs
        /// </summary>
        static uint GetIdleTime()
        {
            LASTINPUTINFO lii = new LASTINPUTINFO();
            
            //setting the structur size is a must-do for Windows API
            lii.cbSize = (uint)Marshal.SizeOf(lii);
            
            //calling API
            if (!GetLastInputInfo(ref lii))
                return 0;

            //current systemtime (ms since start/boot)
            uint tickCount = (uint)Environment.TickCount;

            //difference = idle time
            return tickCount - lii.dwTime;
        }

        /// <summary>
        /// calc. color based on the progress:
        /// 0.0 -> green
        /// 1.0 -> red
        /// </summary>
        /// <param name="progress"></param>
        static Color ColorFromProgress(double progress)
        {
            int r = (int)(255 * progress);              //grows with progress
            int g = (int)(255 * (1 - progress));        //regresses with progress

            return Color.FromArgb(r, g, 0);
        }

        /// <summary>
        /// creates a 16x16 Tray-Icon with the progress-bar
        /// background: circle (with a color)
        /// foreground: "pie-chart" progress (clockwise)
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="color"></param>
        static Icon CreateProgressIcon(double progress, Color color)
        {
            using (Bitmap bmp = new Bitmap(16, 16))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);

                    // color for background circle (always visible)
                    g.FillEllipse(Brushes.LightBlue, 0, 0, 16, 16);


                    // foreground: progress-circle
                    using (Brush brush = new SolidBrush(color))
                    {
                        float sweepAngle = (float)(360 * progress);
                        g.FillPie(brush, 0, 0, 16, 16, -90, sweepAngle);    // filling starts at top (12 o'clock) at -90°
                    }
                }


                // Create HICON from bitmap (unmanaged resource!)
                // Must be destroyed manually, otherwise GDI leak (new icon every second)
                IntPtr hIcon = bmp.GetHicon();

                try
                {
                    using (Icon temp = Icon.FromHandle(hIcon))
                    {
                        return (Icon)temp.Clone();
                    }
                }

                finally
                {
                    DestroyIcon(hIcon);
                }
            }
        }
    
    }
}
