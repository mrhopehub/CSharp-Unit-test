using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataFormat
{
    class Program
    {
        static void Main(string[] args)
        {
            byte b = 0xa0;
            int tmp = Convert.ToInt32(b);
            Console.WriteLine((tmp / 10) % 10);
            Console.ReadKey();
        }
    }
}
