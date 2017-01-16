using System;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;

namespace Lillian.Lib
{
    public delegate object ParamsFunc(params object[] vals);
    public static class Builtin
    {
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

        public static object Loop(params object[] vals)
        {
            if (vals.Length < 2)
                throw new ArgumentException($"{nameof(Loop)} expects two arguments.");

            var iterations = vals[0] as int?;
            if (iterations == null)
                throw new ArgumentException($"{nameof(Loop)}: first argument must be an int.");

            try
            {
                var body = vals[1] as dynamic; // Should be a Func of some type
                object result = "";
                for (var i = 0; i < iterations; i++)
                {
                    result = body.Invoke();
                }
                return result;
            }
            catch (RuntimeBinderException)
            {
                throw new ArgumentException($"{nameof(Loop)}: second argument must be invokable.");
            }
        }
    }
}
