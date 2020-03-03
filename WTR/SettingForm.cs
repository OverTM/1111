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
        public event setTextValue setFormTextValue;

        public SettingForm()
        {
            InitializeComponent();
        }

        WTR form1 = new WTR();

        private void button1_Click(object sender, EventArgs e)
        {
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
            form1.PointX = int.Parse(this.textBox1.Text);
            form1.PointY = int.Parse(this.textBox2.Text);
            setFormTextValue(form1.PointX, form1.PointY);
            Settings1.Default.PointX = form1.PointX;
            Settings1.Default.PointY = form1.PointY;
            Settings1.Default.Save();
        }
    }
}
