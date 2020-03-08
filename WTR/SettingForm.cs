using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WTR
{
    public partial class SettingForm : Form
    {
        public bool AutoCheckUpdade;

        //用于保存主窗体对象引用
        private WTR mainForm = null;
        //从外部将主窗体对象“注入”进来
        public SettingForm(WTR main)
        {
            InitializeComponent();
            mainForm = main;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WTR form1 = new WTR();
            form1.SelectFolder();
            textBox3.Text = Settings1.Default.SavePath;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int x, y;
            if (String.IsNullOrEmpty(textBox1.Text.Trim()) || String.IsNullOrEmpty(textBox2.Text.Trim()))
            {
                MessageBox.Show("不可以为空哦", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                if (int.TryParse(textBox1.Text, out x) && int.TryParse(textBox2.Text, out y))
                {
                    mainForm.SetLocation(x, y);
                }
                else
                {
                    MessageBox.Show("别乱输哦", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void SettingForm_Load(object sender, EventArgs e)
        {
            label1.Text = string.Empty;
            textBox3.Text = Settings1.Default.SavePath;
            textBox1.Text = Settings1.Default.PointX.ToString();
            textBox2.Text = Settings1.Default.PointY.ToString();
            comboBox1.SelectedIndex = Settings1.Default.NetTime ? 1 : 0;
            checkBox1.Checked = true;
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            Settings1.Default.NetTime = comboBox1.SelectedIndex == 1;
            Settings1.Default.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label1.Text = "字体及颜色设置不保存，\n重启软件即可恢复为默认字体";
            label1.ForeColor = Color.Red;
            if (this.fontDialog1.ShowDialog() == DialogResult.OK)
            {
                Font font = this.fontDialog1.Font;
                mainForm.SetFont(font);
            }
            label1.Text = string.Empty;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            label1.Text = "字体及颜色设置不保存，\n重启软件即可恢复为默认字体";
            label1.ForeColor = Color.Red;
            if (this.colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color color = this.colorDialog1.Color;
                mainForm.SetColor(color);
            }
            label1.Text = string.Empty;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Update update = new Update();
            update.IgnoreThisUpdate = 0;
            update.UpdateCheck();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            AutoCheckUpdade = checkBox1.Checked;
            Settings1.Default.AutoCheckUpdade = AutoCheckUpdade;
            Settings1.Default.Save();
        }
    }
}
