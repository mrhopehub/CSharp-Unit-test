using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App;

namespace Action_BeginInvoke
{
    class PassParametersByObject
    {
        private string _name;
        private string _age;
        private string _sex;
        public PassParametersByObject(string name, string age, string sex)
        {
            this._name = name;
            this._age = age;
            this._sex = sex;
        }
        public void run()
        {
            (app.mainForm as Form1).textBox1Writeline(_name + "    " + _age + "    " + _sex);
        }
    }
}
