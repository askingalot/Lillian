using System;
using System.Linq;

namespace Lillian.Lib
{
    public delegate object ParamsFunc(params object[] vals);
    public static class Builtin
    {
        /*
        public static string Print(object val1)
        {
            return PrintAll(val1);
        }
        */

        public static string Print(params object[] vals)
        {
            if (vals != null)
            {
                foreach (var val in vals)
                {
                    Console.Write(val);
                }
            }
            return "";
        }

        public static string PrintLine(params object[] vals)
        {
            Print(vals);
            Console.WriteLine();
            return "";
        }

        public static string Concat(params object[] vals)
        {
            return vals == null
                ? ""
                : string.Concat(vals.Select(v => v ?? ""));
        }
    }
}
