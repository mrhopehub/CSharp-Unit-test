using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using PluginContracts;
using App;
using MEFPlugin;

namespace SimplePlugin
{
    class Program
    {
        static void Main(string[] args)
        {
            Plugins<IPlugin> plugins = new Plugins<IPlugin>();
            plugins.LoadFrom(@"./");
            plugins[1].Do();

            Plugins<IMyPlugin> myPlugins = new Plugins<IMyPlugin>();
            myPlugins.LoadFrom(@"./");
            foreach (IMyPlugin plugin in myPlugins)
            {
                plugin.DIY();
            }

            GenericMEFPluginLoader<IPlugin> loader = new GenericMEFPluginLoader<IPlugin>(@"./MEFPlugins");
            List<IPlugin> MEFPlugins = loader.Plugins;

            foreach(var item in MEFPlugins)
            {
                item.Do();
            }
            MEFPlugins[0].Do();

            Console.ReadKey();
        }
    }
}
