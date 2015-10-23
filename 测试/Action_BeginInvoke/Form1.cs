using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Action_BeginInvoke
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = "单击测试按钮";
            textBox1Writeline(message);
        }

        public void textBox1Writeline(string line)
        {
            Action action = () =>
            {
                this.textBox1.Text += line;
                this.textBox1.Text += "\r\n";
            };
            this.BeginInvoke(action);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread tt = new Thread(() => Hello());
            tt.IsBackground = true;
            tt.Start();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Thread tt = new Thread(() => Hello1("Li YunChong"));
            tt.IsBackground = true;
            tt.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PassParametersByObject threadObj = new PassParametersByObject(this, "李运崇", "26", "男");
            System.Threading.Thread tt = new Thread(() => threadObj.run());
            tt.IsBackground = true;
            tt.Start();
        }

        private void Hello()
        {
            textBox1Writeline("hello world!");
        }

        private void Hello1(object obj)
        {
            textBox1Writeline("hello world,my name is " + (obj as string));
        }
    }
}
