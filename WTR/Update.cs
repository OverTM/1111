using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WTR
{
    class Update
    {
        string pLocalFilePath;
        string pNewVersionPath;
        /// <summary>
        /// 服务器上exe所在文件夹
        /// </summary>
        public string SharePath = @"E:\WorkSpace\log1";
        /// <summary>
        /// 当前程序版本,更新版本后需要修改
        /// </summary>
        public double LocalVersion = 3.1;
        public double IgnoreThisUpdate = Settings1.Default.IgnoreThisUpdate;

        public void UpdateCheck()
        {
            try
            {
                double NV = NewVersion();
                @pLocalFilePath = this.GetType().Assembly.Location;//当前程序的完整路径
                @pNewVersionPath = string.Format(@"{0}\WTR_{1}.exe", SharePath, NV);

                if (NV > LocalVersion && NV > IgnoreThisUpdate)
                {
                    if (MessageBox.Show("发现“考勤记录”的新版本，是否更新？", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        //将新版本复制到当前程序路径下
                        string w = @Path.GetDirectoryName(@pLocalFilePath) + @"\" + Path.GetFileName(@pNewVersionPath);
                        System.IO.File.Copy(@pNewVersionPath, w, true);

                        //通过批处理文件完成改名、重启等
                        CreateBatch(@pLocalFilePath);
                        Process.Start(@Path.GetDirectoryName(@pLocalFilePath) + @"\" + "Restart.bat");
                    }
                    else
                    {
                        IgnoreThisUpdate = NV;
                        Settings1.Default.IgnoreThisUpdate = NV;
                        Settings1.Default.Save();
                    }
                }
                else
                {
                    if (IgnoreThisUpdate == 0)
                    {
                        MessageBox.Show(string.Format("当前版本Ver{0}为最新版，\n无需更新。", LocalVersion));
                    }
                }
            }
            catch
            {
                MessageBox.Show("更新失败，请稍后重试", "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 找到服务器上的最新版号
        /// </summary>
        /// <returns>服务器上的最新版号</returns>
        public double NewVersion()
        {
            try
            {
                string[] AllExe = Directory.GetFiles(SharePath, "*.exe");//各版本的完整路径

                double f, newVersion = 0;
                foreach (string Exe in AllExe)
                {
                    f = double.Parse(Path.GetFileNameWithoutExtension(Exe).Split('_')[1]);
                    newVersion = (f > newVersion) ? f : newVersion;
                }
                return newVersion;
            }
            catch
            {
                MessageBox.Show("访问服务器失败");
                return LocalVersion;
            }
        }

        //创建出来的vbs不起作用
        private void CreatVBS_(string path)
        {
            path = @Path.GetDirectoryName(@path) + @"\" + "Restart.vbs";
            FileStream fs = new FileStream(@path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            //sw.WriteLine("ping 1.1.1.1 -n 1 -w 60000 > nul");//暂停一分钟
            sw.WriteLine("Dim Wsh");
            sw.WriteLine("Set Wsh = WScript.CreateObject(\"WScript.Shell\")");
            sw.WriteLine("Wsh.Run \"taskkill / f / im WTR.exe\",0");
            sw.WriteLine("Set fso = CreateObject(\"Scripting.FileSystemObject\")");
            sw.WriteLine("fso.deleteFile \"WTR.exe\" ");
            sw.WriteLine(string.Format("set f=fso.getfile({0})", Path.GetFileName(@pNewVersionPath)));
            sw.WriteLine("f.name=\"WTR.exe\"");
            sw.WriteLine("Wsh.Run \"WTR.exe\",1,false");
            sw.Write("fso.deleteFile \"Restart.vbs\" ");
            sw.Close();
            fs.Close();
        }

        /// <summary>
        /// 通过vbs调用批处理文件，做到不显示cmd窗口，但可能会被报病毒
        /// </summary>
        /// <param name="path"></param>
        private void CreatVBS(string path)
        {
            path = @Path.GetDirectoryName(@path) + @"\" + "NoShowCMD.vbs";
            FileStream fs = new FileStream(@path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            //sw.WriteLine("ping 1.1.1.1 -n 1 -w 60000 > nul");//暂停一分钟
            sw.WriteLine("Set shell = Wscript.createobject(\"wscript.shell\")");
            sw.Write("shell.run \"Restart.bat\",0");
            sw.Close();
            fs.Close();
            CreateBatch(path);
        }
        private void CreateBatch(string path)
        {
            path = @Path.GetDirectoryName(@path) + @"\" + "Restart.bat";
            FileStream fs = new FileStream(@path, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            //开始写入
            //sw.WriteLine("ping 1.1.1.1 -n 1 -w 60000 > nul");//暂停一分钟
            sw.WriteLine("@echo Please do not close this window manually, otherwise the update will fail");
            //sw.WriteLine("@echo 更新中。このウィンドウを手動で閉じないでください。");
            sw.WriteLine("@echo off");
            sw.WriteLine("taskkill /f /im WTR.exe");
            sw.WriteLine(@"del /f /s /q " + @pLocalFilePath);
            sw.WriteLine(@"ren " + @Path.GetDirectoryName(@pLocalFilePath) + @"\" + Path.GetFileName(@pNewVersionPath) + " WTR.exe");
            sw.WriteLine("start " + @pLocalFilePath);
            //sw.WriteLine("pause");
            sw.Write(@"del /f /s /q " + @path);
            sw.Close();
            fs.Close();
        }
    }

    public partial class WTR : Form
    {
        //将新版本复制到共享盘
        partial void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Update update = new Update();
                if (textBox1.Text == "0123456789")
                {
                    double NV = update.NewVersion();
                    if (NV < update.LocalVersion)
                    {
                        string LP = this.GetType().Assembly.Location;
                        string NP = string.Format(@"{0}\WTR_{1}.exe", update.SharePath, update.LocalVersion);
                        File.Copy(LP, NP, true);
                        MessageBox.Show(string.Format("更新到{0}成功,\n新版本文件名为\"WTR_{1}\"", NP, update.LocalVersion));
                    }
                    else
                    {
                        MessageBox.Show(string.Format("本程序版本号：{0}\n服务器上的最新版本号：{1}\n无需更新到服务器！", update.LocalVersion, NV));
                    }
                }
                else
                {
                    MessageBox.Show("你没有更新权限");
                }
            }
            catch
            {
                MessageBox.Show("更新失败，请稍后重试", "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
