using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using WTR.Properties;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace WTR
{
    public partial class WTR : Form
    {
        public WTR()
        {
            InitializeComponent();
        }

        string SavePath, StartWorkTime;
        int PointX, PointY;
        static bool NetTime;

        #region 时间
        string FullTime, YearMonth, YearMonthDay, HourMinuteSecond;
        private void GetTime(ref string FullTime, ref string YearMonth, ref string YearMonthDay, ref string HourMinuteSecond )
        {
            DateTime GetedTime = DateTime.MinValue;
            switch (System.Threading.Thread.CurrentThread.CurrentCulture.Name)
            {
                case "ja-JP":
                    if(Settings1.Default.NetTime)
                    {
                        try
                        {
                            string output;
                            System.Diagnostics.Process p = new System.Diagnostics.Process();
                            p.StartInfo.FileName = "cmd.exe";
                            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                            p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                            p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                            p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                            p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                            p.Start();//启动程序

                            //向cmd窗口发送输入信息
                            p.StandardInput.WriteLine(@"net time " + "&exit"); //设置文件的格式: net_time

                            p.StandardInput.AutoFlush = true;

                            //获取cmd窗口的输出信息
                            output = p.StandardOutput.ReadToEnd();

                            p.WaitForExit();
                            p.Close();
                            output = output.Split(new char[] { 'は', 'で' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                            GetedTime = Convert.ToDateTime(output);
                        }
                        catch
                        {
                            MessageBox.Show("获取时间失败，可能是网线没有插好","错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        GetedTime = DateTime.Now;
                    }
                    break;

                case "zh-CN"://自己电脑上调试用
                    if (调试模式ToolStripMenuItem.Checked)
                    {
                        GetedTime = DateTime.Now;
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("只支持日语系统");
                    }
                    break;
                default:
                    System.Windows.Forms.MessageBox.Show("只支持日语系统");
                    break;
            }

            FullTime = GetedTime.ToString("yyyy/MM/dd HH:mm:ss");
            YearMonth = GetedTime.ToString("yyyyMM") + "_";
            YearMonthDay = GetedTime.ToString("yyyy/MM/dd");
            HourMinuteSecond = GetedTime.ToString("HH:mm:ss");
        }
        #endregion

        /// <summary>
        /// 监控屏幕锁定与解锁
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                // 屏幕锁定
                End();
            }

            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                // 屏幕解锁
                Start();
            }
        }
        /// <summary>
        /// 设置log文件路径
        /// </summary>
        /// <returns>log文件路径</returns>
        public void SelectFolder()
        {
            folderBrowserDialog1.Description = "请选择log文件保存路径";
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                SavePath = folderBrowserDialog1.SelectedPath;
                SavePath = SavePath.Replace(@"\", "//");
                Settings1.Default.SavePath = SavePath;
                Settings1.Default.Save();
            }
            else if (Settings1.Default.SavePath == "")
            {
                SelectFolder();
            }
            else
            {
                SavePath = Settings1.Default.SavePath;
            }
        }

        /// <summary>
        /// 工作开始后的操作
        /// </summary>
        private void Start()
        {
            GetTime(ref FullTime, ref YearMonth, ref YearMonthDay, ref HourMinuteSecond);
            try
            {
                string Lock = "屏幕解锁时间：";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(SavePath + "//" + YearMonth + "log.txt", true))
                {
                    file.WriteLine(Lock + FullTime);
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 工作结束后的操作
        /// </summary>
        private void End()
        {
            GetTime(ref FullTime, ref YearMonth, ref YearMonthDay, ref HourMinuteSecond);
            try
            {
                string UnLock = "屏幕锁定时间：";
                string q = SavePath + "//" + YearMonth + "log.txt";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(SavePath + "//" + YearMonth + "log.txt", true))
                {
                    file.WriteLine(UnLock + FullTime);
                    file.Close();
                }
                try
                {
                    ReadLog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 读取log，找到昨天下班时间和今天上班时间，存入kaoqin.txt
        /// </summary>
        private void ReadLog()
        {
            string EndWorkTime = "Not found";
            StreamReader sr = new StreamReader(SavePath + "//" + YearMonth + "log.txt");
            bool flag = false;
            string lasttime = "not found";
            while (!sr.EndOfStream)
            {
                string[] date;
                string tempdate = sr.ReadLine();
                date = tempdate.Split(new char[] { '：', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (date[1].Trim() == YearMonthDay)
                {
                    StartWorkTime = date[1] + " " + date[2];
                    flag = true;
                    break;
                }
                else
                {
                    lasttime = date[1] + " " + date[2];
                    flag = false;
                    EndWorkTime = lasttime;
                }
            }
            sr.Close();
            if (flag)
            {
                bool WriteInToKaoqin = false;
                if (File.Exists(SavePath + "//" + YearMonth + "kaoqin.txt"))
                {
                    StreamReader kq = new StreamReader(SavePath + "//" + YearMonth + "kaoqin.txt");
                    while (!kq.EndOfStream)
                    {
                        string[] date;
                        string tempdate = kq.ReadLine();
                        date = tempdate.Split(new char[] { '：', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (date[1].Trim() == YearMonthDay)
                        {
                            WriteInToKaoqin = false;
                            break;
                        }
                        else
                        {
                            WriteInToKaoqin = true;
                        }
                    }
                    kq.Close();
                }
                else
                {
                    WriteInToKaoqin = true;
                }

                if (WriteInToKaoqin)
                {
                    using (System.IO.StreamWriter kq = new System.IO.StreamWriter(SavePath + "//" + YearMonth + "kaoqin.txt", true))
                    {
                        kq.WriteLine("下班时间：" + EndWorkTime);
                        kq.WriteLine("上班时间：" + StartWorkTime);
                        kq.Close();
                        Settings1.Default.StartWorkTime = StartWorkTime;
                        this.label2.Text = "出勤時間:"+StartWorkTime;
                        Settings1.Default.Save();

                        if (Settings1.Default.AutoCheckUpdade)
                        {
                            Update update = new Update();
                            update.UpdateCheck();//每天做一次更新检查
                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Settings1.Default.SavePath == "")
            {
                SelectFolder();
            }
            else
            {
                SavePath = Settings1.Default.SavePath;
            }

            NetTime = Settings1.Default.NetTime;
            StartWorkTime = Settings1.Default.StartWorkTime == "" ? "Not found" : Settings1.Default.StartWorkTime;
            this.label2.Text = "出勤時間:" + StartWorkTime;

            this.button1.BackColor = Color.Yellow;
            调试模式ToolStripMenuItem.Checked = false;
            this.checkBox1.Visible = false;
            this.textBox1.Visible = false;
            调试模式ToolStripMenuItem.Visible = false;
            button2.Visible = false;

            PointX = Settings1.Default.PointX == 0 ? 1600 : Settings1.Default.PointX;
            PointY = Settings1.Default.PointY == 0 ? 80 : Settings1.Default.PointY;
            this.Location = new Point(PointX, PointY);

            SystemEvents.SessionSwitch += new
                SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        #region 调试用
        //private void WTR_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if ((Keys)e.KeyChar == Keys.F11)
        //    {
        //        e.Handled = true;   //将Handled设置为true，指示已经处理过KeyPress事件  
        //        调试模式ToolStripMenuItem.Visible = !调试模式ToolStripMenuItem.Visible;
        //    }  
        //}

        private void WTR_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                //调用Microsoft.VisualBasic，使用VB中的inputbox,实现弹出输入框的功能。
                string str = Interaction.InputBox("请输入密码", "请输入密码", "password", -1, -1);
                if (str == "我要调试")
                {
                    调试模式ToolStripMenuItem.Visible = true;
                }
            }
        }

        bool Lock = true;
        private void button1_Click(object sender, EventArgs e)
        {
            if (Lock)
            {
                End();
                this.button1.BackColor = Color.Red;
                this.button1.Text = "Lock";
            }
            else
            {
                Start();
                this.button1.BackColor = Color.Yellow;
                this.button1.Text = "Unlock";
            }
            Lock = !Lock;
        }
        private void 调试模式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            if (调试模式ToolStripMenuItem.Checked)
            {
                this.button1.Visible = true;
                this.button2.Visible = true;
                this.BackColor = System.Drawing.Color.Green;
                this.TransparencyKey = System.Drawing.Color.YellowGreen;
                this.checkBox1.Visible = true;
                this.textBox1.Visible = true;
            }
            else
            {
                this.button1.Visible = false;
                this.button2.Visible = false;
                this.BackColor = System.Drawing.Color.White;
                this.TransparencyKey = System.Drawing.Color.White;
                this.checkBox1.Visible = false;
                this.textBox1.Visible = false;
                调试模式ToolStripMenuItem.Visible = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            checkBox1.Text = string.Format("每隔{0}ms自动点击一次左边的按钮", textBox1.Text);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //只能输入数字和退格
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }

        private void checkBox1_CheckStateChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                while(true)
                {
                    button1.PerformClick();
                    Thread.Sleep(int.Parse(textBox1.Text));
                }
            }
        }

        /// <summary>
        /// 将新版本复制到共享盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        partial void button2_Click(object sender, EventArgs e);
        #endregion

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public static SettingForm form;//声明窗体类的静态变量
        private void 设置ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //防止重复打开
            if (form == null || form.IsDisposed)
            {
                form = new SettingForm(this);//加入this用于传location值，主窗体把自己的引用传给从窗体对象
                form.Show();
            }
            else
                form.Activate();
        }
        public void SetLocation(int X,int Y)
        {
            this.Location = new Point(X, Y);
            Settings1.Default.PointX = X;
            Settings1.Default.PointY = Y;
            Settings1.Default.Save();
        }
        public void SetFont(Font font)
        {
            this.label2.Font = font;
        }
        public void SetColor(Color color)
        {
            this.label2.ForeColor = (Color)color;
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }
        #region 设置全局热键
        public class AppHotKey
        {
            [DllImport("kernel32.dll")]
            public static extern uint GetLastError();
            //如果函数执行成功，返回值不为0。
            //如果函数执行失败，返回值为0。要得到扩展错误信息，调用GetLastError。
            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool RegisterHotKey(
                IntPtr hWnd,                //要定义热键的窗口的句柄
                int id,                     //定义热键ID（不能与其它ID重复）          
                KeyModifiers fsModifiers,   //标识热键是否在按Alt、Ctrl、Shift、Windows等键时才会生效
                Keys vk                     //定义热键的内容
                );

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool UnregisterHotKey(
                IntPtr hWnd,                //要取消热键的窗口的句柄
                int id                      //要取消热键的ID
                );

            //定义了辅助键的名称（将数字转变为字符以便于记忆，也可去除此枚举而直接使用数值）
            [Flags()]
            public enum KeyModifiers
            {
                None = 0,
                Alt = 1,
                Ctrl = 2,
                Shift = 4,
                WindowsKey = 8
            }
            /// <summary>
            /// 注册热键
            /// </summary>
            /// <param name="hwnd">窗口句柄</param>
            /// <param name="hotKey_id">热键ID</param>
            /// <param name="keyModifiers">组合键</param>
            /// <param name="key">热键</param>
            public static void RegKey(IntPtr hwnd, int hotKey_id, KeyModifiers keyModifiers, Keys key)
            {
                try
                {
                    if (!RegisterHotKey(hwnd, hotKey_id, keyModifiers, key))
                    {
                        if (Marshal.GetLastWin32Error() == 1409) { MessageBox.Show("热键被占用 ！"); }
                        else
                        {
                            MessageBox.Show("注册热键失败！");
                        }
                    }
                }
                catch (Exception) { }
            }
            /// <summary>
            /// 注销热键
            /// </summary>
            /// <param name="hwnd">窗口句柄</param>
            /// <param name="hotKey_id">热键ID</param>
            public static void UnRegKey(IntPtr hwnd, int hotKey_id)
            {
                //注销Id号为hotKey_id的热键设定
                UnregisterHotKey(hwnd, hotKey_id);
            }
        }
        private const int WM_HOTKEY = 0x312; //窗口消息-热键
        private const int WM_CREATE = 0x1; //窗口消息-创建
        private const int WM_DESTROY = 0x2; //窗口消息-销毁

        private const int Space = 0x3572; //热键ID
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case WM_HOTKEY: //窗口消息-热键ID
                    switch (m.WParam.ToInt32())
                    {
                        case Space: //热键ID
                            this.Visible = true;
                            this.WindowState = FormWindowState.Normal;//正常大小
                            this.Activate(); //激活窗体
                            //MessageBox.Show("我按了Control +Shift +Alt +S");//设置按下热键后的动作
                            break;
                        default:
                            break;
                    }
                    break;
                case WM_CREATE: //窗口消息-创建
                    AppHotKey.RegKey(Handle, Space, AppHotKey.KeyModifiers.Ctrl, Keys.Space); //热键为Ctrl+空格
                    break;
                case WM_DESTROY: //窗口消息-销毁
                    AppHotKey.UnRegKey(Handle, Space); //销毁热键
                    break;
                default:
                    break;
            }
        }
        #endregion

    }
}
