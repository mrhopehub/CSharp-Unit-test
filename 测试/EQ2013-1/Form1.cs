using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EQ2013_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            EQ2013_1.init();
        }

        private void btSendData_Click(object sender, EventArgs e)
        {
            EQ2013_1.ShowInfo(this.textBox1.Text.ToString(), 60);
            MessageBox.Show("发送数据成功");
        }
    }
}
