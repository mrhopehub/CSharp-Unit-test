using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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


    public class Plugins<T>
    {
        private List<T> _plugins = new List<T>();

        public IEnumerator<T> GetEnumerator()
        {
            return _plugins.GetEnumerator();
        }
        public void Add(T plugin)
        {
            _plugins.Add(plugin);
        }
        public void Remove(T plugin)
        {
            _plugins.Remove(plugin);
        }
        public T this[int index]
        {
            get
            {
                return _plugins[index];
            }
            set
            {
                _plugins[index] = value;
            }
        }
        public void LoadFrom(string path)
        {
            string[] dllFileNames = null;
            if (Directory.Exists(path))
            {
                dllFileNames = Directory.GetFiles(path, "*.dll");
            }
            List<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames)
            {
                Assembly assembly = Assembly.LoadFrom(dllFile);
                assemblies.Add(assembly);
            }

            Type pluginType = typeof(T);
            List<Type> pluginTypes = new List<Type>();

            foreach (Assembly assembly in assemblies)
            {
                if (assembly != null)
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if (type.GetInterface(pluginType.FullName) != null)
                            {
                                pluginTypes.Add(type);
                            }
                        }
                    }
                }
            }
            foreach (Type type in pluginTypes)
            {
                T plugin = (T)Activator.CreateInstance(type);
                _plugins.Add(plugin);
            }
        }
    }
}
