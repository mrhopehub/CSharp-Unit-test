using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginContracts
{
    public interface IPlugin
    {
        string Name { get; }
        void Do();
    }

    public interface IMyPlugin
    {
        string Name { get; }
        void DIY();
    }
}

