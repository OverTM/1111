using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.TransparencyKey = System.Drawing.Color.White;
            this.BackColor = System.Drawing.Color.White;
            InitializeComponent();
            asd();
        }

        [DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll", EntryPoint = "SetParent")]
        static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, ref Rectangle lpRect);
        [DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);
        static IntPtr hShell = FindWindowEx(IntPtr.Zero, IntPtr.Zero, "Shell_TrayWnd", null);
        static IntPtr hBar = FindWindowEx(hShell, IntPtr.Zero, "ReBarWindow32", null);
        static IntPtr hMin = FindWindowEx(hBar, IntPtr.Zero, "MSTaskSwWClass", null);

        /// <summary>
        /// îC??é⁄ê°éÊìæ
        /// </summary>
        private void asd()
        {
            Rectangle rcShell = new Rectangle();
            Rectangle rcBar = new Rectangle();
            Rectangle rcMin = new Rectangle();
            GetWindowRect(hShell, ref rcShell);
            GetWindowRect(hBar, ref rcBar);
            GetWindowRect(hMin, ref rcMin);
            SetParent(this.Handle, hMin);
            Rectangle screen = System.Windows.Forms.SystemInformation.VirtualScreen;
            int sWidth = screen.Width;
            int sHeight = screen.Height;

            MoveWindow(hMin, 0, 0, rcMin.Right - rcMin.Left - this.Width, rcMin.Bottom - rcMin.Top, true);
            MoveWindow(this.Handle, 1500, 12, this.Width, this.Height, true);

        }
    }
}
