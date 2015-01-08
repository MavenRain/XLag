using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.QualityTools.NetworkEmulation;

namespace XLag
{
    class Main
    {
        private NotifyIcon tray;
        private ContextMenu menu;
        private MenuItem start;
        private MenuItem stop;
        private MenuItem exit;
        private Icon onIcon;
        private Icon offIcon;
        private NetworkEmulationDriver driver;
        private bool running;

        public void Initialize()
        {
            Assembly thisAssembly = Assembly.GetAssembly(typeof(Main));
            using (Stream stream = thisAssembly.GetManifestResourceStream("XLag.XLagon.ico"))
            {
                onIcon = new Icon(stream);
            }
            using (Stream stream = thisAssembly.GetManifestResourceStream("XLag.XLagoff.ico"))
            {
                offIcon = new Icon(stream);
            }

            tray = new NotifyIcon();
            tray.Icon = offIcon;
            tray.Text = "XLag - Off";

            start = new MenuItem { Text = "Start" };

            string[] profiles = Directory.GetFiles(string.Format("{0}{1}{2}", Application.StartupPath, Path.DirectorySeparatorChar, "Profiles"));
            for (int i = 0; i < profiles.Length; i++)
            {
                MenuItem profileItem = new MenuItem() { Text = Path.GetFileNameWithoutExtension(profiles[i]) };
                profileItem.Click += profileItem_Click;
                start.MenuItems.Add(profileItem);
            }

            stop = new MenuItem { Text = "Stop", Enabled = false };
            stop.Click += stop_Click;

            exit = new MenuItem { Text = "Exit" };
            exit.Click += exit_Click;

            menu = new ContextMenu();
            menu.MenuItems.Add(new MenuItem { Text = string.Format("XLag v{0}", Application.ProductVersion), Enabled = false });
            menu.MenuItems.Add(start);
            menu.MenuItems.Add(stop);
            menu.MenuItems.Add(exit);

            tray.ContextMenu = menu;
            tray.Visible = true;
        }

        private void ExitWithError(string title, string message)
        {
            MessageBox.Show(string.Format("{0}\n\nThings to check:\n1. Make sure the VS2013 network emulator driver is installed.\n2. Make sure this program is running as administrator.\n\nPress OK to terminate the program.", message), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            tray.Visible = false;
            Application.Exit();
        }

        void profileItem_Click(object sender, EventArgs e)
        {
            if (driver == null)
            {
                try
                {
                    driver = new NetworkEmulationDriver();
                    if (!driver.Initialize())
                    {
                        ExitWithError("Failed to Initialize Driver", "NetworkEmulationDriver.Initialize returned false.");
                        return;
                    }
                }
                catch (NetworkEmulationException exception)
                {
                    ExitWithError("Failed to Initialize Driver", exception.Message);
                    return;
                }
            }
            MenuItem menuItem = (MenuItem)sender;
            string profileLocation = string.Format("{0}{1}Profiles{1}{2}.network", Application.StartupPath, Path.DirectorySeparatorChar, menuItem.Text);
            try
            {
                if (!driver.LoadProfile(profileLocation))
                {
                    ExitWithError("Failed to Load Profile", "NetworkEmulationDriver.LoadProfile returned false.");
                    return;
                }
            }
            catch (NetworkEmulationException exception)
            {
                ExitWithError("Failed to Load Profile", exception.Message);
                return;
            }
            try
            {
                if (!driver.StartEmulation())
                {
                    ExitWithError("Failed to Start Emulation", "NetworkEmulationDriver.StartEmulation returned false.");
                    return;
                }
            }
            catch (NetworkEmulationException exception)
            {
                ExitWithError("Failed to Start Emulation", exception.Message);
                return;
            }
            tray.Text = string.Format("XLag - On ({0})", menuItem.Text);
            tray.Icon = onIcon;
            start.Enabled = false;
            stop.Enabled = true;
            running = true;
        }

        private void stop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!driver.StopEmulation())
                {
                    ExitWithError("Failed to Stop Emulation", "NetworkEmulationDriver.StopEmulation returned false.");
                    return;
                }
            }
            catch (NetworkEmulationException exception)
            {
                ExitWithError("Failed to Stop Emulation", exception.Message);
                return;
            }
            tray.Text = string.Format("XLag - Off");
            tray.Icon = offIcon;
            stop.Enabled = false;
            start.Enabled = true;
            running = false;
        }

        private void exit_Click(object sender, EventArgs e)
        {
            if (driver != null)
            {
                if (running)
                {
                    try
                    {
                        if (!driver.StopEmulation())
                        {
                            ExitWithError("Failed to Stop Emulation", "NetworkEmulationDriver.StopEmulation returned false.");
                            return;
                        }
                    }
                    catch (NetworkEmulationException exception)
                    {
                        ExitWithError("Failed to Stop Emulation", exception.Message);
                        return;
                    }
                }
                try
                {
                    if (!driver.Cleanup())
                    {
                        ExitWithError("Failed to Cleanup", "NetworkEmulationDriver.Cleanup returned false.");
                        return;
                    }
                }
                catch (NetworkEmulationException exception)
                {
                    ExitWithError("Failed to Cleanup", exception.Message);
                    return;
                }
            }
            tray.Visible = false;
            Application.Exit();
        }
    }
}
