using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log
{
    class Program
    {
        static void Main(string[] args)
        {
            Log log = new Log();
            Console.WriteLine(log.ReadLog(2021, 01, 23)?"Done!":"Error!");
            Console.ReadKey();
        }
    }
}
