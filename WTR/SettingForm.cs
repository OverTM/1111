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
        public SettingForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WTR form1 = new WTR();
            form1.SelectFolder();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            SaveLocation();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            SaveLocation();
        }

        public delegate void setTextValue(int X,int Y);

        public void SaveLocation()
        {
            WTR.PointX = int.Parse(this.textBox1.Text);
            WTR.PointY = int.Parse(this.textBox2.Text);
            Settings1.Default.PointX = WTR.PointX;
            Settings1.Default.PointY = WTR.PointY;
            Settings1.Default.Save();
        }
    }
}
