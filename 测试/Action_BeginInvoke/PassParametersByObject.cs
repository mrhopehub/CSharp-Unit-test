using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Action_BeginInvoke
{
    class PassParametersByObject
    {
        private Form1 _form;
        private string _name;
        private string _age;
        private string _sex;
        public PassParametersByObject(Form1 form, string name, string age, string sex)
        {
            this._form = form;
            this._name = name;
            this._age = age;
            this._sex = sex;
        }
        public void run()
        {
            this._form.textBox1Writeline(_name + "    " + _age + "    " + _sex);
        }
    }
}
