using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTR
{
    class Updata
    {
        string pLocalFilePath;
        string pNewVersionPath;
        float LocalVersion = Settings1.Default.LocalVersion;

        public void UpdataCheck()
        {
            @pLocalFilePath = this.GetType().Assembly.Location;
            @pNewVersionPath = Directory.GetFiles(@"\\10.8.1.196\psv\temp\zhaokun\tools\wtr", "*.exe")[0];

            float NewVersion = float.Parse(Path.GetFileNameWithoutExtension(@pNewVersionPath).Split('_')[1]);
            if (NewVersion > LocalVersion)
            {
                System.IO.File.Copy(@pNewVersionPath, @Path.GetDirectoryName(@pLocalFilePath) + "WTR_new.exe", true);

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine(@"del /f /s /q " + @pLocalFilePath); 
                p.StandardInput.WriteLine(@"ren" + @pLocalFilePath + "WTR.exe" + "&exit"); //多条命令可以分成多行或用 & 区分
                //p.StandardInput.WriteLine(@"ren E:\std\WTR\q2.txt q1.txt"); //设置文件的格式: net_time

                p.StandardInput.AutoFlush = true;
                p.WaitForExit();
                p.Close();
            }
        }
    }
}
