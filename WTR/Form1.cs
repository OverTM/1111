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

namespace WTR
{
    public partial class WTR : Form
    {
        public WTR()
        {
            InitializeComponent();
        }

        enum TimeType
        {
            FullTime,
            YearMonthDay,
            HourMinuteSecond
        }

        string SavePath, StartWorkTime;
        public static int PointX, PointY;

        #region 调用CMD获取局域网指定电脑时间
        private string GetTime(TimeType timeType)
        {
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
            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();
            p.Close();

            switch (System.Threading.Thread.CurrentThread.CurrentCulture.Name)
            {
                case "ja-JP":
                    output = output.Split(new char[] { 'は', 'で' }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
                    break;
                default:
                    System.Windows.Forms.MessageBox.Show("只支持日语系统");
                    break;
            }

            string[] ArrayTime = output.Split(new char[] { '/', ' ', ':' }, StringSplitOptions.RemoveEmptyEntries);
            string fullTime = output.Trim();
            string yearMonthDay = (ArrayTime[0] + "/" + ArrayTime[1] + "/" + ArrayTime[2]).Trim();
            string hourMinuteSecond = (ArrayTime[3] + "：" + ArrayTime[4] + "：" + ArrayTime[5]).Trim();

            try
            {
                switch (timeType)
                {
                    case TimeType.FullTime:
                        return fullTime;
                        break;
                    case TimeType.YearMonthDay:
                        return yearMonthDay;
                        break;
                    case TimeType.HourMinuteSecond:
                        return hourMinuteSecond;
                        break;
                    default:
                        return "error";
                        break;
                }
            }
            catch
            {
                MessageBox.Show("获取时间失败，可能是网线没有插好");
                return "error";
            }
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
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择log文件保存路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SavePath = dialog.SelectedPath;
                SavePath = SavePath.Replace(@"\", "//");
            }
            else
            {
                MessageBox.Show("设置失败,请重新设置");
                SelectFolder();
            }
            Settings1.Default.SavePath = SavePath;
            Settings1.Default.Save();
        }

        /// <summary>
        /// 工作开始后的操作
        /// </summary>
        private void Start()
        {
            try
            {
                string Lock = "屏幕解锁时间：";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(SavePath + "/log.txt", true))
                {
                    file.WriteLine(Lock + GetTime(TimeType.FullTime));
                    file.Close();
                }
            }
            catch
            {
                MessageBox.Show("将屏幕解锁时间写入log文件时失败");
            }
        }

        /// <summary>
        /// 工作结束后的操作
        /// </summary>
        private void End()
        {
            try
            {
                string UnLock = "屏幕锁定时间：";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(SavePath + "/log.txt", true))
                {
                    file.WriteLine(UnLock + GetTime(TimeType.FullTime));
                    file.Close();
                }
                try
                {
                    ReadLog();
                }
                catch
                {
                    MessageBox.Show("读取log或者写入考勤文件时失败");
                }
            }
            catch
            {
                MessageBox.Show("将屏幕锁定时间写入log文件时失败");
            }
        }

        /// <summary>
        /// 读取log，找到昨天下班时间和今天上班时间，存入kaoqin.txt
        /// </summary>
        private void ReadLog()
        {
            string EndWorkTime = "Not found";
            StreamReader sr = new StreamReader(SavePath + "/log.txt");
            bool flag = false;
            string lasttime = "not found";
            while (!sr.EndOfStream)
            {
                string[] date;
                string tempdate = sr.ReadLine();
                date = tempdate.Split(new char[] { '：', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (date[1].Trim() == GetTime(TimeType.YearMonthDay))
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
                if (File.Exists(SavePath + "/kaoqin.txt"))
                {
                    StreamReader kq = new StreamReader(SavePath + "/kaoqin.txt");
                    while (!kq.EndOfStream)
                    {
                        string[] date;
                        string tempdate = kq.ReadLine();
                        date = tempdate.Split(new char[] { '：', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (date[1].Trim() == GetTime(TimeType.YearMonthDay))
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
                    using (System.IO.StreamWriter kq = new System.IO.StreamWriter(SavePath + "/kaoqin.txt", true))
                    {
                        kq.WriteLine("下班时间：" + EndWorkTime);
                        kq.WriteLine("上班时间：" + StartWorkTime);
                        kq.Close();
                        Settings1.Default.StartWorkTime = StartWorkTime;
                        this.label2.Text = StartWorkTime;
                        Settings1.Default.Save();
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
            StartWorkTime = Settings1.Default.StartWorkTime == "" ? "Not found" : Settings1.Default.StartWorkTime;
            this.label2.Text = StartWorkTime;

            调试模式ToolStripMenuItem.Checked = false;

            PointX = Settings1.Default.PointX == 0 ? 1600 : Settings1.Default.PointX;
            PointY = Settings1.Default.PointY == 0 ? 80 : Settings1.Default.PointY;
            this.Location = new Point(PointX, PointY);

            SystemEvents.SessionSwitch += new
                SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        #region 调试用
        bool Lock = false;
        private void button1_Click(object sender, EventArgs e)
        {
            Lock = !Lock;
            if(Lock)
            {
                End();
                this.button1.Text = "Lock";
            }
            else
            {
                Start();
                this.button1.Text = "Unlock";
            }
        }
        private void 调试模式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (sender as ToolStripMenuItem).Checked = !(sender as ToolStripMenuItem).Checked;
            if (调试模式ToolStripMenuItem.Checked)
            {
                this.button1.Visible = true;
                this.BackColor = System.Drawing.Color.Green;
                this.TransparencyKey = System.Drawing.Color.YellowGreen;
            }
            else
            {
                this.button1.Visible = false;
                this.BackColor = System.Drawing.Color.White;
                this.TransparencyKey = System.Drawing.Color.White;
            }
        }
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
                form = new SettingForm();
                form.Show();
            }
            else
                form.Activate();
        }
        void SetLocation(int X,int Y)
        {
            this.Location = new Point(X, Y);
        }


        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        }
    }
}
