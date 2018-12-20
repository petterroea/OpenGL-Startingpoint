using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solskogen2017.DIAGNOSTICS
{
    class Debug
    {
        public Debug()
        {

        }
        public static void DebugConsole(string from, string data)
        {
            Console.WriteLine("[ "+from+" ] "+data);
        }
    }
}
