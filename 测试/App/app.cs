using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace App
{
    public static class app
    {
        public static Form mainForm;

        static app()
        {
            mainForm = (Form)Control.FromHandle(Process.GetCurrentProcess().MainWindowHandle);
        }
    }
}
