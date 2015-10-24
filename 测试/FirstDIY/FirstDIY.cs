using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginContracts;

namespace FirstDIY
{
    public class FirstDIY:IMyPlugin
    {
        public string Name { 
            get{
                return "FirstDIY";
            } 
        }
        public void DIY()
        {
            Console.WriteLine(Name);
        }
    }
}
