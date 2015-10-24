using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginContracts;

namespace SecondPlugin
{
    public class SecondPlugin : IPlugin
    {
        #region IPlugin Members

        public string Name
        {
            get
            {
                return "Second Plugin";
            }
        }

        public void Do()
        {
            System.Windows.Forms.MessageBox.Show("Do Something in Second Plugin");
        }

        #endregion
    }
}

