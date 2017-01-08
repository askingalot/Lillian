using System.Collections.Generic;
using static System.Console;

namespace Lillian
{
    public delegate void ParamsAction(params object[] vals);
    public static class Builtin
    {
        public static void Print(params object[] vals)
        {
            if (vals == null) return;
            foreach (var val in vals)
            {
                Write(val);
            }
        }


        public static void Hello()
        {
            Write("Hello, World!");
        }
    }
}
