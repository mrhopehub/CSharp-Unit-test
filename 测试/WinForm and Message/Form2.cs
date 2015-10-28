using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinForm_and_Message
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        //禁止窗口最大化、最小化、关闭
        protected override void WndProc(ref Message m)
        {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_MINIMIZE = 0xF020;
            const int SC_MAXIMIZE = 0xF030;
            const int SC_CLOSE = 0xF060;
            if (m.Msg == WM_SYSCOMMAND && ((int)m.WParam == SC_MINIMIZE || (int)m.WParam == SC_MAXIMIZE || (int)m.WParam == SC_CLOSE))
            {
                MessageBox.Show("操作的子窗口");
                return;
            }
            base.WndProc(ref m);
        }
    }
}
