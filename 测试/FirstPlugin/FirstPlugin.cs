using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginContracts;

namespace FirstPlugin
{
    public class FirstPlugin : IPlugin
    {
        #region IPlugin Members

        public string Name
        {
            get
            {
                return "First Plugin";
            }
        }

        public void Do()
        {
            System.Windows.Forms.MessageBox.Show("Do Something in First Plugin");
        }

        #endregion
    }
}

