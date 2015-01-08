using System;
using System.Windows.Forms;
using System.Threading;

namespace XLag
{
    class Program
    {
        [STAThread]
        private static void Main()
        {
            System.Threading.Thread.CurrentThread.Name = "Main Thread";
            Console.WriteLine("XLag " + Application.ProductVersion + " started");
            if (!BitConverter.IsLittleEndian)
            {
                MessageBox.Show("XLag does not support big-endian architectures.\n\nPress OK to terminate the program.", "XLag: Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string mutex_id = "Global\\XLAG";
            using (Mutex mutex = new Mutex(false, mutex_id))
            {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("XLag is already running. It is not possible to run this program twice. Close the other instance to open a new one.", "XLag: Already Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Main main = new Main();
                main.Initialize();
                Application.Run();
            }
        }
    }
}
