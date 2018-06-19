using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;

namespace War.Base
{
    public class StringEx
    {
        private static StringBuilder sb = new StringBuilder();
        public static string Format(string format, params object[] args)
        {
            if ((format == null) || (args == null))
            {
                //throw new ArgumentNullException((format == null) ? "format" : "args");
                return format;
            }
            int len = format.Length;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is string)
                    len += ((string)args[i]).Length;
                else if (args[i] is int)
                    len += 16;
                else if (args[i] is double)
                    len += 16;
                else if (args[i] is float)
                    len += 16;
                else
                    len += 8;
            }
            if (len > sb.Capacity)
                sb.Capacity = len;
            sb.Length = 0;
            sb.AppendFormat(format, args);
            return sb.ToString();
        }
    }
}